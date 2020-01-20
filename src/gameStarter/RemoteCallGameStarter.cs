using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LobbyAPI.Http;
using LobbyAPI.Models;

namespace LobbyAPI.GameStarter
{
    public class RemoteCallGameStarter : IGameStarterProvider
    {
        public string RegisteredName => "RemoteCall";
        readonly string gameServerUri;
        readonly ISerializedHttpClientProvider client;
        public RemoteCallGameStarter(IProviderConfiguration<IGameStarterProvider> configuration, ISerializedHttpClientProvider client)
        {
            if (configuration == null || client == null)
            {
                throw new ArgumentException();
            }
            gameServerUri = configuration["Uri"];
            this.client = client;
        }

        public async Task<string> StartGame(string gameType, IEnumerable<string> players)
        {
            if (gameType == null || players == null)
            {
                throw new ArgumentException();
            }
            var body = new StartGameRequest
            {
                Players = players.ToArray(),
                Type = gameType
            };
            var response = await client.SendAsync<StartGameResponse>(HttpMethod.Post, gameServerUri, body);
            return response.Id;
        }
    }
}