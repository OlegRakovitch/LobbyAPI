using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RattusAPI.Configuration;
using RattusAPI.Http;
using RattusAPI.Models;

namespace RattusAPI.GameStarter
{
    public class RemoteCallGameStarter : IGameStarterProvider
    {
        public string RegisteredName => "RemoteCall";
        readonly string gameServerUri;
        readonly ISerializedHttpClientProvider client;
        public RemoteCallGameStarter(IConfiguration configuration, ISerializedHttpClientProvider client)
        {
            gameServerUri = configuration.Value("Providers:GameStarter:Uri");

            this.client = client;
        }

        public async Task<string> StartGame(string gameType, IEnumerable<string> players)
        {
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