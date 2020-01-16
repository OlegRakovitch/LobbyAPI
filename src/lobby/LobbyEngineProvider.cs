using LobbyAPI.Context;
using LobbyAPI.GameStarter;

namespace LobbyAPI.Lobby
{
    public class LobbyEngineProvider : LobbyEngine.Lobby, ILobbyEngineProvider
    {
        public string RegisteredName => "Contextual";

        public LobbyEngineProvider(IContextProvider context, IGameStarterProvider gameStarter) : base(context, gameStarter)
        {

        }
    }
}