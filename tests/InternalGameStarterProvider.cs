using System.Collections.Generic;
using System.Threading.Tasks;
using RattusAPI.GameStarter;

namespace RattusAPI.Tests
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