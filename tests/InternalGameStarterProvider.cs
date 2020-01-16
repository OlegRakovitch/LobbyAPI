using System.Collections.Generic;
using System.Threading.Tasks;
using LobbyAPI.GameStarter;

namespace LobbyAPI.Tests
{
    public class InternalGameStarterProvider : IGameStarterProvider
    {
        public string RegisteredName => "Internal";

        public Task<string> StartGame(string gameType, IEnumerable<string> players)
        {
            return Task.FromResult("IdOfStartedGame");
        }
    }
}