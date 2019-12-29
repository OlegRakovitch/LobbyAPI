using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using RattusAPI.Views;
using Xunit;

namespace RattusAPI.Tests
{
    public class LobbyControllerTests : IClassFixture<WebApplicationFactory<RattusAPI.Startup>>
    {
        readonly HttpClient client;

        public LobbyControllerTests(WebApplicationFactory<RattusAPI.Startup> factory)
        {
            client = factory.CreateClient();
            SetupApplication().GetAwaiter().GetResult();
        }

        async Task SetupApplication()
        {
            var context = GetAuthenticationContext("admin");
            var response = await SendAsync(context, HttpMethod.Post, "/api/admin/reset");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        AuthenticationContext GetAuthenticationContext(string username)
        {
            return new AuthenticationContext("username", username);
        }

        async Task<HttpResponseMessage> SendAsync(AuthenticationContext context, HttpMethod method, string path, string content = null)
        {
            using (var requestMessage = new HttpRequestMessage(method, path))
            {
                if (!string.IsNullOrEmpty(context.HeaderName))
                {
                    requestMessage.Headers.Add(context.HeaderName, context.HeaderValue);
                }
                if (!string.IsNullOrEmpty(content))
                {
                    requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
                }
                return await client.SendAsync(requestMessage);
            }
        }


        [Fact]
        public async Task UserCanNotRequestRoomsAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinRoomAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotLeaveRoomAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await SendAsync(context, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotStartGameAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await SendAsync(context, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanJoinRoomIfNotInRoom()
        {
            var ownerContext = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var userContext = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var getResponse = await SendAsync(userContext, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Single(views);
            var view = views.Single();
            Assert.Equal("InRoom", view.State);
            Assert.Equal("room", view.Name);
            Assert.Equal(new string[] { "owner", "user" }, view.Players);
            Assert.Equal(2, view.PlayersCount);
            Assert.Equal("owner", view.Owner);
            Assert.False(view.IsOwner);
        }

        [Fact]
        public async Task UserCanNotJoinNonExistingRoom()
        {
            var context = GetAuthenticationContext("user");
            var response = await SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinFullRoom()
        {
            var user1Context = GetAuthenticationContext("user1");
            var user1Response = await SendAsync(user1Context, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, user1Response.StatusCode);

            var user2Context = GetAuthenticationContext("user2");
            var user2Response = await SendAsync(user1Context, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, user1Response.StatusCode);

            var user3Context = GetAuthenticationContext("user3");
            var user3Response = await SendAsync(user1Context, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, user1Response.StatusCode);

            var user4Context = GetAuthenticationContext("user4");
            var user4Response = await SendAsync(user1Context, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, user1Response.StatusCode);

            var user5Context = GetAuthenticationContext("user5");
            var user5Response = await SendAsync(user1Context, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, user1Response.StatusCode);
        }

        [Fact]
        public async Task UserBecomesPlayerAndOwnerOfCreatedRoom()
        {
            var context = GetAuthenticationContext("user");
            var createResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var getResponse = await SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Single(views);
            var view = views.Single();
            Assert.Equal("InRoom", view.State);
            Assert.Equal("room", view.Name);
            Assert.Equal(new string[] { "user" }, view.Players);
            Assert.Equal(1, view.PlayersCount);
            Assert.Equal("user", view.Owner);
            Assert.True(view.IsOwner);
        }

        [Fact]
        public async Task UsersCanNotCreateRoomsWithTheSameName()
        {
            var user1Context = GetAuthenticationContext("user1");
            var user1Response = await SendAsync(user1Context, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, user1Response.StatusCode);

            var user2Context = GetAuthenticationContext("user2");
            var user2Response = await SendAsync(user2Context, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, user2Response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomWhenUserIsInRoom()
        {
            var ownerContext = GetAuthenticationContext("owner");
            var ownerResponse = await SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, ownerResponse.StatusCode);

            var userContext = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var createResponse = await SendAsync(userContext, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinSameRoomIfUserIsAlreadyInRoom()
        {
            var context = GetAuthenticationContext("user");
            var createResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var joinResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinDifferentRoomIfUserIsAlreadyInRoom()
        {
            var owner = GetAuthenticationContext("owner");
            var ownerResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, ownerResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var createResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room2""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
        }

        [Fact]
        public async Task UserCanLeaveRoomIfUserIsInRoom()
        {
            var owner = GetAuthenticationContext("owner");
            var ownerResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, ownerResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var leaveResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.OK, leaveResponse.StatusCode);

            var getResponse = await SendAsync(user, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Single(views);
            var view = views.Single();
            Assert.Equal("Joinable", view.State);
            Assert.Equal("room", view.Name);
            Assert.Null(view.Players);
            Assert.Equal(1, view.PlayersCount);
            Assert.Null(view.Owner);
            Assert.False(view.IsOwner);
        }

        [Fact]
        public async Task UserCanNotLeaveRoomIfUserIsNotInRoom()
        {
            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
        }

        [Fact]
        public async Task UserCanNotLeaveRoomIfGameAlreadyStarted()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var startResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Accepted, startResponse.StatusCode);

            var leaveResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.Forbidden, leaveResponse.StatusCode);
        }

        [Fact]
        public async Task OwnerCanStartGameIfRoomHasTwoOrMorePlayers()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var startResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Accepted, startResponse.StatusCode);

            var getResponse = await SendAsync(owner, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Single(views);
            var view = views.Single();
            Assert.Equal("InGame", view.State);
            Assert.Equal("room", view.Name);
            Assert.Equal(new string[] { "owner", "user" }, view.Players);
            Assert.Equal(2, view.PlayersCount);
            Assert.Equal("owner", view.Owner);
            Assert.True(view.IsOwner);
        }

        [Fact]
        public async Task UserCanNotStartGameIfUserIsNotOwner()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var startResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, startResponse.StatusCode);
        }

        [Fact]
        public async Task UserCanNotStartGameIfUserIsNotInRoom()
        {
            var context = GetAuthenticationContext("user");
            var response = await SendAsync(context, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task OwnerCanNotStartGameWithoutOtherPlayers()
        {
            var context = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var startResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, startResponse.StatusCode);
        }

        [Fact]
        public async Task OwnerCanNotStartAlreadyStartedGame()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var startResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Accepted, startResponse.StatusCode);

            var secondStartResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, startResponse.StatusCode);
        }

        [Fact]
        public async Task RoomGetsDeletedIfOwnerLeavesRoom()
        {
            var context = GetAuthenticationContext("user");
            var createResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var leaveResponse = await SendAsync(context, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            var getResponse = await SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Empty(views);
        }

        [Fact]
        public async Task UserGetsEmptyListOfRoomsIfThereAreNoRooms()
        {
            var context = GetAuthenticationContext("user");
            var response = await SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Empty(views);
        }

        [Fact]
        public async Task RoomHasInGameStatusIfOwnerStartedGame()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var startResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Accepted, startResponse.StatusCode);

            var ownerGetResponse = await SendAsync(owner, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, ownerGetResponse.StatusCode);
            var ownerContent = await ownerGetResponse.Content.ReadAsStringAsync();
            var ownerViews = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(ownerContent);
            Assert.Single(ownerViews);
            var ownerView = ownerViews.Single();
            Assert.Equal("InGame", ownerView.State);
            Assert.Equal("room", ownerView.Name);
            Assert.Equal(new string[] { "owner", "user" }, ownerView.Players);
            Assert.Equal(2, ownerView.PlayersCount);
            Assert.Equal("owner", ownerView.Owner);
            Assert.True(ownerView.IsOwner);

            var userGetResponse = await SendAsync(owner, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, userGetResponse.StatusCode);
            var userContent = await userGetResponse.Content.ReadAsStringAsync();
            var userViews = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(userContent);
            Assert.Single(userViews);
            var userView = userViews.Single();
            Assert.Equal("InGame", userView.State);
            Assert.Equal("room", userView.Name);
            Assert.Equal(new string[] { "owner", "user" }, userView.Players);
            Assert.Equal(2, userView.PlayersCount);
            Assert.Equal("owner", userView.Owner);
            Assert.False(userView.IsOwner);
        }

        [Fact]
        public async Task RoomHasInRoomStatusIfOwnerCreatedRoom()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var ownerGetResponse = await SendAsync(owner, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, ownerGetResponse.StatusCode);
            var ownerContent = await ownerGetResponse.Content.ReadAsStringAsync();
            var ownerViews = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(ownerContent);
            Assert.Single(ownerViews);
            var ownerView = ownerViews.Single();
            Assert.Equal("InRoom", ownerView.State);
            Assert.Equal("room", ownerView.Name);
            Assert.Equal(new string[] { "owner", "user" }, ownerView.Players);
            Assert.Equal(2, ownerView.PlayersCount);
            Assert.Equal("owner", ownerView.Owner);
            Assert.True(ownerView.IsOwner);

            var userGetResponse = await SendAsync(owner, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, userGetResponse.StatusCode);
            var userContent = await userGetResponse.Content.ReadAsStringAsync();
            var userViews = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(userContent);
            Assert.Single(userViews);
            var userView = userViews.Single();
            Assert.Equal("InRoom", userView.State);
            Assert.Equal("room", userView.Name);
            Assert.Equal(new string[] { "owner", "user" }, userView.Players);
            Assert.Equal(2, userView.PlayersCount);
            Assert.Equal("owner", userView.Owner);
            Assert.False(userView.IsOwner);
        }

        [Fact]
        public async Task RoomHasJoinableStatusIfUserIsNotInRoom()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var getResponse = await SendAsync(user, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Single(views);
            var view = views.Single();
            Assert.Equal("Joinable", view.State);
            Assert.Equal("room", view.Name);
            Assert.Null(view.Players);
            Assert.Equal(1, view.PlayersCount);
            Assert.Null(view.Owner);
            Assert.False(view.IsOwner);
        }

        [Fact]
        public async Task UserCanSeeListOfPlayersOfJoinedOnlyRoom()
        {
            var owner = GetAuthenticationContext("owner");
            var createResponse = await SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var user = GetAuthenticationContext("user");
            var firstGetResponse = await SendAsync(user, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, firstGetResponse.StatusCode);
            var firstContent = await firstGetResponse.Content.ReadAsStringAsync();
            var firstViews = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(firstContent);
            Assert.Single(firstViews);
            var firstView = firstViews.Single();
            Assert.Equal("Joinable", firstView.State);
            Assert.Equal("room", firstView.Name);
            Assert.Null(firstView.Players);
            Assert.Equal(1, firstView.PlayersCount);
            Assert.Null(firstView.Owner);
            Assert.False(firstView.IsOwner);

            var joinResponse = await SendAsync(user, HttpMethod.Post, "/api/lobby", @"{{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

            var secondGetResponse = await SendAsync(user, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, secondGetResponse.StatusCode);
            var secondContent = await secondGetResponse.Content.ReadAsStringAsync();
            var secondViews = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(secondContent);
            Assert.Single(secondViews);
            var secondView = secondViews.Single();
            Assert.Equal("Joinable", secondView.State);
            Assert.Equal("room", secondView.Name);
            Assert.Equal(new string[] { "owner", "user" }, secondView.Players);
            Assert.Equal(1, secondView.PlayersCount);
            Assert.Equal("owner", secondView.Owner);
            Assert.False(secondView.IsOwner);
        }
    }
}
