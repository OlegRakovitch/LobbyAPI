namespace RattusAPI.Views
{
    public class RoomView
    {
        public string Name { get; set; }
        public string State { get; set; }
        public int PlayersCount { get; set; }
        public string[] Players { get; set; }
        public string Owner { get; set; }
        public bool IsOwner { get; set; }
    }
}