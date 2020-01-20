namespace LobbyAPI
{
    public interface IProviderConfiguration<IProvider>
    {
        string this[string key] { get; }
    }
}