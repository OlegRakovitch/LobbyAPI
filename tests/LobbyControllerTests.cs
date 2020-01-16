using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RattusAPI.Views;
using Xunit;

namespace RattusAPI.Tests
{
    public class LobbyControllerTests : IClassFixture<AppFactory<TestsStartup>>
    {
        readonly RequestHelper helper;

        public LobbyControllerTests(AppFactory<TestsStartup> factory)
        {
            helper = new RequestHelper(factory.UseOriginalStartup().CreateClient());
            SetupApplication().GetAwaiter().GetResult();
        }

        async Task SetupApplication()
        {
            var context = helper.GetAuthenticationContext("admin");
            await helper.SendAsync(context, HttpMethod.Post, "/api/admin/reset");
        }

        [Fact]
        public async Task UserCanNotRequestRoomsAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await helper.SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinRoomAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotLeaveRoomAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotStartGameAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanJoinRoomIfNotInRoom()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinNonExistingRoom()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinFullRoom()
        {
            var user1Context = helper.GetAuthenticationContext("user1");
            await helper.SendAsync(user1Context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user2Context = helper.GetAuthenticationContext("user2");
            await helper.SendAsync(user2Context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var user3Context = helper.GetAuthenticationContext("user3");
            await helper.SendAsync(user3Context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var user4Context = helper.GetAuthenticationContext("user4");
            await helper.SendAsync(user4Context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var user5Context = helper.GetAuthenticationContext("user5");
            var user5Response = await helper.SendAsync(user5Context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, user5Response.StatusCode);
        }

        [Fact]
        public async Task UserCanCreateRoomIfUserIsNotInRoom()
        {
            var context = helper.GetAuthenticationContext("user");
            var createResponse = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var getResponse = await helper.SendAsync(context, HttpMethod.Get, "/api/lobby");
            var content = await getResponse.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("room", view.Name);
        }

        [Fact]
        public async Task UserBecomesPlayerAndOwnerOfCreatedRoom()
        {
            var context = helper.GetAuthenticationContext("user");
            await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var response = await helper.SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal(new string[] { "user" }, view.Players);
            Assert.Equal(1, view.PlayersCount);
            Assert.Equal("user", view.Owner);
            Assert.True(view.IsOwner);
        }

        [Fact]
        public async Task CreatedRoomHasSpecifiedName()
        {
            var context = helper.GetAuthenticationContext("user");
            await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var response = await helper.SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("room", view.Name);
        }

        [Fact]
        public async Task UsersCanNotCreateRoomsWithTheSameName()
        {
            var user1Context = helper.GetAuthenticationContext("user1");
            await helper.SendAsync(user1Context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user2Context = helper.GetAuthenticationContext("user2");
            var response = await helper.SendAsync(user2Context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomWhenUserIsInRoom()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            await helper.SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var response = await helper.SendAsync(userContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinSameRoomIfUserIsAlreadyInRoom()
        {
            var context = helper.GetAuthenticationContext("user");
            await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinDifferentRoomIfUserIsAlreadyInRoom()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room2"",""type"":""game""}");

            var response = await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanLeaveRoomIfUserIsInRoom()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var leaveResponse = await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.OK, leaveResponse.StatusCode);

            var getResponse = await helper.SendAsync(user, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("Joinable", view.State);
        }

        [Fact]
        public async Task UserCanNotLeaveRoomIfUserIsNotInRoom()
        {
            var user = helper.GetAuthenticationContext("user");
            var joinResponse = await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
        }

        [Fact]
        public async Task UserCanNotLeaveRoomIfGameAlreadyStarted()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/start");

            var response = await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/leave");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task OwnerCanStartGameIfRoomHasTwoOrMorePlayers()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var startResponse = await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Accepted, startResponse.StatusCode);

            var getResponse = await helper.SendAsync(owner, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var content = await getResponse.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("InGame", view.State);
        }

        [Fact]
        public async Task UserCanNotStartGameIfUserIsNotOwner()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var response = await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotStartGameIfUserIsNotInRoom()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task OwnerCanNotStartGameWithoutOtherPlayers()
        {
            var context = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task OwnerCanNotStartAlreadyStartedGame()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            
            var response = await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RoomGetsDeletedIfOwnerLeavesRoom()
        {
            var context = helper.GetAuthenticationContext("user");
            await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/leave");
            
            var response = await helper.SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Empty(views);
        }

        [Fact]
        public async Task UserGetsEmptyListOfRoomsIfThereAreNoRooms()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Get, "/api/lobby");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var views = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content);
            Assert.Empty(views);
        }

        [Fact]
        public async Task RoomHasInGameStatusIfOwnerStartedGame()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/start");
            
            var response = await helper.SendAsync(owner, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("InGame", view.State);
        }

        [Fact]
        public async Task RoomHasInRoomStatusIfOwnerCreatedRoom()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");
            
            var response = await helper.SendAsync(owner, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("InRoom", view.State);
        }

        [Fact]
        public async Task RoomHasJoinableStatusIfUserIsNotInRoom()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(user, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("Joinable", view.State);
        }

        [Fact]
        public async Task UserCanSeeListOfPlayersOfJoinedOnlyRoom()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");
            var firstGetResponse = await helper.SendAsync(user, HttpMethod.Get, "/api/lobby");
            var firstContent = await firstGetResponse.Content.ReadAsStringAsync();
            var firstView = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(firstContent).Single();
            Assert.Null(firstView.Players);
            Assert.Equal(1, firstView.PlayersCount);

            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var secondGetResponse = await helper.SendAsync(user, HttpMethod.Get, "/api/lobby");
            var secondContent = await secondGetResponse.Content.ReadAsStringAsync();
            var secondView = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(secondContent).Single();
            Assert.Equal(new string[] { "owner", "user" }, secondView.Players);
            Assert.Equal(2, secondView.PlayersCount);
        }

        [Fact]
        public async Task UserCanSeeRoomOwnerOfJoinedOnlyRoom()
        {
            var owner = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(owner, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var user = helper.GetAuthenticationContext("user");

            var firstGetResponse = await helper.SendAsync(user, HttpMethod.Get, "/api/lobby");
            var firstContent = await firstGetResponse.Content.ReadAsStringAsync();
            var firstView = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(firstContent).Single();
            Assert.Null(firstView.Owner);
            Assert.False(firstView.IsOwner);

            await helper.SendAsync(user, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var secondGetResponse = await helper.SendAsync(user, HttpMethod.Get, "/api/lobby");
            var secondContent = await secondGetResponse.Content.ReadAsStringAsync();
            var secondView = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(secondContent).Single();
            Assert.Equal("owner", secondView.Owner);
            Assert.False(secondView.IsOwner);
        }

        [Fact]
        public async Task UserCanSeeRoomNameBeforeJoiningRoom()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(userContext, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("room", view.Name);
        }

        [Fact]
        public async Task UserCanSeeRoomNameAfterJoiningRoom()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            await helper.SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var response = await helper.SendAsync(userContext, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("room", view.Name);
        }

        [Fact]
        public async Task UserCanSeeGameTypeBeforeJoiningRoom()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(userContext, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("game", view.GameType);
        }

        [Fact]
        public async Task UserCanSeeGameTypeAfterJoiningRoom()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            await helper.SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            var response = await helper.SendAsync(userContext, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("game", view.GameType);
        }

        [Fact]
        public async Task UserCanSeeGameIdAfterGameWasStarter()
        {
            var ownerContext = helper.GetAuthenticationContext("owner");
            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""game""}");

            var userContext = helper.GetAuthenticationContext("user");
            await helper.SendAsync(userContext, HttpMethod.Post, "/api/lobby/join", @"{""name"":""room""}");

            await helper.SendAsync(ownerContext, HttpMethod.Post, "/api/lobby/start");

            var response = await helper.SendAsync(userContext, HttpMethod.Get, "/api/lobby");
            var content = await response.Content.ReadAsStringAsync();
            var view = JsonConvert.DeserializeObject<IEnumerable<RoomView>>(content).Single();
            Assert.Equal("IdOfStartedGame", view.GameId);
        }

        [Fact]
        public async Task UserCanNotCreateRoomWithoutName()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""type"":""game""}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomWithEmptyName()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":"""",""type"":""game""}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomWithoutGameType()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room""}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotCreateRoomWithEmptyGameType()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/create", @"{""name"":""room"",""type"":""""}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinRoomWithoutName()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotJoinRoomWithEmptyName()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/lobby/join", @"{""name"":""""}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
