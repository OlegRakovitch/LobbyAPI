using RattusAPI.Context;
using RattusAPI.GameStarter;

namespace RattusAPI.LobbyEngine
{
    public class LobbyEngineProvider : RattusEngine.LobbyEngine, ILobbyEngineProvider
    {
        public string RegisteredName => "Contextual";

        public LobbyEngineProvider(IContextProvider context, IGameStarterProvider gameStarter) : base(context, gameStarter)
        {

        }
    }
}