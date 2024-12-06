using Steamworks;
using System.Numerics;

namespace Fishy.Models
{
    public class Player
    {
        public SteamId SteamID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public long InstanceID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }


        public Player(SteamId id, string name)
        {
            SteamID = id;
            string randomID = new(Enumerable.Range(0, 3).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[new Random().Next(36)]).ToArray());
            ID = randomID;
            Name = name;

            InstanceID = 0;
            Position = new Vector3(0, 0, 0);
        }
    }
}
