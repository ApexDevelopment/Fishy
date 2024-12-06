using Fishy.Utils;
using Steamworks;

namespace Fishy.Chat
{
    public enum PermissionLevel
    {
        Player = 0,
        Admin = 1,
        Server = 100,
    }

    class CommandHandler
    {
        public static Dictionary<string, Command> Commands = [];

        public static void AddCommand(Command cmd)
            => Commands.Add(cmd.Name, cmd);

        public static void Init()
        {
            AddCommand(new Commands.HelpCommand());
            AddCommand(new Commands.RulesCommand());
            AddCommand(new Commands.IssueCommand());
            AddCommand(new Commands.ReportCommand());
            AddCommand(new Commands.SpawnCommand());
            AddCommand(new Commands.CodeOnlyCommand());
            AddCommand(new Commands.BanCommand());
            AddCommand(new Commands.KickCommand());
            AddCommand(new Commands.SetAdmin());
            AddCommand(new Commands.RevokeAdmin());
            AddCommand(new Commands.TeleportCommand());
        }
       
        public static int GetPermissionLevel(SteamId player) // Temporary, ideally will have ranks
        {
            if (player == SteamClient.SteamId) // If player is server
                return (int)PermissionLevel.Server;

            return Fishy.Config.Admins.Contains(player.ToString()) ? (int)PermissionLevel.Admin : (int)PermissionLevel.Player;
        }

        public static bool OnMessage(SteamId from, string message)
        {
            if (!message.StartsWith('!'))
                return false;

            string commandName = message.Remove(0, 1).Split(" ")[0];
            string[] arguments = message.Split(" ").Skip(1).ToArray();

            if (!Commands.TryGetValue(commandName, out Command? cmd))
            {
                ChatUtils.SendChat(from, $"Command \"{commandName}\" does not exist!", "bb1111");
                return true;
            }

            if ((int)cmd.PermissionLevel > GetPermissionLevel(from))
            {
                ChatUtils.SendChat(from, $"You don't have permission to use \"{commandName}\"!", "bb1111");
                return true;
            }

            ChatLogger.Log(new ChatMessage(default, $"Player {from} executed command {commandName} with arguments {string.Join(", ", arguments)}"));

            try
            {
                cmd.OnUse(from, arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while running command \"{commandName}\" {ex}");
            }
            return true;
        }
    }

    public abstract class Command
    {
        public virtual string Name { get; set; } = "";
        public virtual string[] Aliases { get; set; } = [];
        public virtual PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Player;
        public virtual string Description { get; set; } = "";
        public virtual string Help { get; set; } = "";

        public virtual void OnUse(SteamId player, string[] arguments) { }
    }

}
