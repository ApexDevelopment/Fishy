using Fishy.Extensions;
using Fishy.Models;
using Fishy.Models.Packets;
using Fishy.Utils;
using Steamworks;
using System.Numerics;

namespace Fishy
{
    class Fishy
    {
        internal static Config Config = new();
        internal static World World = new();
        internal static List<Player> Players = [];
        internal static List<Actor> Actors = [];
        internal static SteamHandler SteamHandler = new();
        internal static NetworkHandler NetworkHandler = new();
        internal static List<string> BannedUsers = [];
        public static List<Dictionary<Vector2, int>> CanvasData = [];
        internal static List<FishyExtension> Extensions = [];
        static readonly string configPath = Path.Combine(AppContext.BaseDirectory, "config.toml");
        static readonly string bansPath = Path.Combine(AppContext.BaseDirectory, "bans.txt");

        public static void Init()
        {

            Console.WriteLine("Fishy - Your Dedicated Webfishing Server");
            Console.WriteLine("Starting Server");
            Console.WriteLine("Reading config file...");
            LoadConfig();
            Console.WriteLine("Reading world file...");
            LoadWorld();
            Console.WriteLine("Initializing Steam Client...");
            InitSteam();
            Console.WriteLine("Reading Banned players...");
            LoadBannedPlayers();
            Console.WriteLine("Starting NetworkHandler...");
            NetworkHandler.Start();
            Console.WriteLine("NetworkHandler was started successfully");
            Console.WriteLine("Creating Lobby...");
            SteamHandler.CreateLobby();
            Console.WriteLine("Loading Extensions...");
            Extensions = ExtensionLoader.GetExtensions();
            foreach(FishyExtension extension in Extensions)
            {
                Console.WriteLine($"Loading Extension: {extension.GetType()}");
                extension.OnInit();
            }
            Console.WriteLine($"{Extensions.Count} Extensions were loaded");
            ListenForInput();
        }

        static void ListenForInput()
        {
            while (true) { 
                string? message = Console.ReadLine();
                if (!String.IsNullOrEmpty(message))
                {
                    if (message.StartsWith("!"))
                    {
                        Console.WriteLine(DateTime.Now.ToString("dd.MM HH:mm:ss") + " Executing command.");
                        CommandHandler.OnMessage(SteamClient.SteamId, message);
                    }
                    else
                        new MessagePacket("Server: " + message).SendPacket("all", (int)CHANNELS.GAME_STATE);
                }
            }
        }

        static void LoadConfig()
        {

            if (!File.Exists(configPath))
            {
                Console.WriteLine("No config file found. (config.toml) Shutting down...");
                Environment.Exit(1);
            }

            if (!Config.LoadConfig(configPath))
            {
                Console.WriteLine("Error in config file. Shutting down...");
                Environment.Exit(1);
            }
            Console.WriteLine("Config was read successfully");
        }

        static void LoadWorld()
        {
            string worldPath = Path.Combine(AppContext.BaseDirectory, "Worlds", Config.World);
            if (!File.Exists(worldPath))
            {
                Console.WriteLine("No world file found. (main_zone.tscn) Shutting down...");
                Environment.Exit(1);
            }

            if (!World.LoadWorld(worldPath))
            {
                Console.WriteLine("Error in world file. Shutting down...");
                Environment.Exit(1);
            }
            Console.WriteLine("Worldfile was read successfully");
        }

        static void LoadBannedPlayers()
        {
            if (!File.Exists(bansPath))
                File.Create(bansPath);

            using StreamReader banReader = new(bansPath);
            while (!banReader.EndOfStream)
                BannedUsers.Add(banReader.ReadLine() ?? "");

            SteamHandler.SetSteamBanList(BannedUsers);

            Console.WriteLine("Bans were read successfully");
        }

        static void InitSteam()
        {
            string error = SteamHandler.Init();
            if (!String.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error Initializing Steam Client. Shutting down...");
                Console.WriteLine("Error: " + error);
                Environment.Exit(1);
            }
            Console.WriteLine("Steam Client was initialized successfully");
        }

    }
}
