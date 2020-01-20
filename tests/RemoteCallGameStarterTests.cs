using System;
using System.Collections.Generic;
using System.Net.Http;
using LobbyAPI.GameStarter;
using LobbyAPI.Http;
using Xunit;

namespace LobbyAPI.Tests
{
    public class RemoteCallGameStarterTests : IClassFixture<AppFactory<TestsStartup>>
    {
        [Fact]
        public async void GameCanBeStarted()
        {
            var configuration = new InternalConfiguration<IGameStarterProvider>(new Dictionary<string, string>() { { "Uri", "/api/gamestarter" } });
            var clientProvider = new InternalHttpClientProvider();
            clientProvider.RegisterResponse("/api/gamestarter", @"{""Id"":""GameId""}");
            var serializedClientProvider = new JsonClientProvider(clientProvider);
            var starter = new RemoteCallGameStarter(configuration, serializedClientProvider);
            var gameId = await starter.StartGame("game", new string[] { "Player1", "Player2" });
            Assert.Equal("GameId", gameId);
        }

        [Fact]
        public void GameCanNotBeStartedIfConfigIsNotProvided()
        {
            var clientProvider = new InternalHttpClientProvider();
            clientProvider.RegisterResponse("/api/gamestarter", @"{""Id"":""GameId""}");
            var serializedClientProvider = new JsonClientProvider(clientProvider);
            Assert.Throws<ArgumentException>(() => new RemoteCallGameStarter(null, serializedClientProvider ));
        }

        [Fact]
        public void GameCanNotBeStartedIfClientIsNotProvided()
        {
            var configuration = new InternalConfiguration<IGameStarterProvider>(new Dictionary<string, string>() { { "Uri", "/api/gamestarter" } });
            Assert.Throws<ArgumentException>(() => new RemoteCallGameStarter(configuration, null));
        }

        [Fact]
        public async void GameCanNotBeStartedIfGameTypeIsNotProvided()
        {
            var configuration = new InternalConfiguration<IGameStarterProvider>(new Dictionary<string, string>() { { "Uri", "/api/gamestarter" } });
            var clientProvider = new InternalHttpClientProvider();
            clientProvider.RegisterResponse("/api/gamestarter", @"{""Id"":""GameId""}");
            var serializedClientProvider = new JsonClientProvider(clientProvider);
            var starter = new RemoteCallGameStarter(configuration, serializedClientProvider);
            await Assert.ThrowsAsync<ArgumentException>(() => starter.StartGame(null, new string[] { "Player1", "Player2" }));
        }

        [Fact]
        public async void GameCanNotBeStartedIfPlayersAreNotProvided()
        {
            var configuration = new InternalConfiguration<IGameStarterProvider>(new Dictionary<string, string>() { { "Uri", "/api/gamestarter" } });
            var clientProvider = new InternalHttpClientProvider();
            clientProvider.RegisterResponse("/api/gamestarter", @"{""Id"":""GameId""}");
            var serializedClientProvider = new JsonClientProvider(clientProvider);
            var starter = new RemoteCallGameStarter(configuration, serializedClientProvider);
            await Assert.ThrowsAsync<ArgumentException>(() => starter.StartGame("game", null));
        }
    }
}