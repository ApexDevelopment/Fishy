﻿using Fishy.Models;
using Fishy.Models.Packets;
using Fishy.Utils;
using Steamworks;

namespace Fishy.Chat.Commands
{
    internal class KickCommand : Command
    {
        public override string Name() =>"kick";
        public override string Description() =>"Kick a player";
        public override ushort PermissionLevel() => 1;
        public override string[] Aliases() => new string[0];

        public override string Help() => "!kick player";
        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help()); return; }

            Player? playerToKick = CommandHandler.FindPlayer(arguments[0]);

            if (playerToKick == null)
                return;

            Punish.KickPlayer(playerToKick);

            ChatUtils.SendChat(executor, $"Kicked player {playerToKick.Name} {playerToKick.SteamID}");
        }
    }
    internal class BanCommand : Command
    {
        public override string Name() => "ban";
        public override string Description() =>"Ban a player";
        public override ushort PermissionLevel() => 1;
        public override string[] Aliases() => new string[0];

        public override string Help() => "!ban player";
        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help()); return; }

            Player? playerToBan = CommandHandler.FindPlayer(arguments[0]);

            if (playerToBan == null)
                return;

            Punish.BanPlayer(playerToBan);

            ChatUtils.SendChat(executor, $"Banned player {playerToBan.Name} {playerToBan.SteamID}");
        }
    }
    public class SpawnCommand : Command
    {
        public override string Name() => "spawn";
        public override string Description() =>"spawn an entity";
        public override ushort PermissionLevel() => 1;

        public override string[] Aliases() => new string[0];

        public override string Help() => "!spawn type\nAvaiable default types: fish, meteor, raincloud, metalspot, voidportal";
        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help()); return; }
            var from = executor;

            switch (arguments[0])
            { 
                case "fish":
                    Spawner.SpawnFish();
                    ChatUtils.SendChat(executor, "A fish has been spawned!");
                    return;
                case "meteor":
                    Spawner.SpawnFish("fish_spawn_alien");
                    ChatUtils.SendChat(executor, "A meteor has been spawned!");
                    return;
                case "rain":
                    Spawner.SpawnRainCloud();
                    ChatUtils.SendChat(executor, "A raincloud has been spawned!");
                    return;
                case "metal":
                    Spawner.SpawnMetalSpot();
                    ChatUtils.SendChat(executor, "A metalspot has been spawned!");
                    return;
                case "void_portal":
                    Spawner.SpawnVoidPortal();
                    ChatUtils.SendChat(executor, "A voidportal has been spawned!");
                    return;
            }

            var player = CommandHandler.FindPlayer(executor);
            if (player == null)
                return;
            Spawner.SpawnActor(new Actor(Spawner.GetFreeId(), arguments[0], player.Position));
        }
    }
    public class CodeOnlyCommand : Command
    {
        public override string Name() => "codeonly";
        public override string Description() =>"sets lobby type";
        public override ushort PermissionLevel() => 1;
        public override string[] Aliases() => new string[0];

        public override string Help() => "!codeonly true/false";
        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help()); return; }
            var from = executor;
            string type = arguments[0] == "true" ? "code_only" : "public";
            Fishy.SteamHandler.Lobby.SetData("type", type);
            new MessagePacket("The lobby type has been set to: " + type).SendPacket("single", (int)CHANNELS.GAME_STATE, from);
        }
    }

    internal class ReportCommand : Command
    {
        public override string Name() => "report";
        public override string Description() => "Report a player";

        public override ushort PermissionLevel() => 0;

        public override string[] Aliases() => new string[0];

        public override string Help() => "!report player reason";
        public override void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length < 2) { ChatUtils.SendChat(executor, Help()); return; }
            var from = executor;
            string reportPath = Path.Combine(AppContext.BaseDirectory, Fishy.Config.ReportFolder, DateTime.Now.ToString("ddMMyyyyHHmmss") + arguments[0] + ".txt");
            string report = "Report for user: " + arguments[0];
            report += "\nReason: " + String.Join(" ", arguments[1..]);
            report += "\nChat Log:\n\n";

            string chatLog = String.Empty;
            Player? player = Fishy.Players.FirstOrDefault(p => p.Name.Equals(arguments[0]));

            if (player != null)
                chatLog = ChatLogger.GetLog(player.SteamID);
            else
                chatLog = ChatLogger.GetLog();

            File.WriteAllText(reportPath, report + ChatLogger.GetLog());
            new MessagePacket(Fishy.Config.ReportResponse, "b30000").SendPacket("single", (int)CHANNELS.GAME_STATE, from);
        }
    }
    internal class IssueCommand : Command
    {
        public override string Name() => "issue";
        public override string Description() =>"Report an issue";
        public override ushort PermissionLevel() => 0;

        public override string Help() => "!issue description";
        public override string[] Aliases() => new string[0];
        public void OnUse(SteamId executor, string[] arguments)
        {
            if (arguments.Length < 1)  { ChatUtils.SendChat(executor, Help()); return; };
            var from = executor;
            string issuePath = Path.Combine(AppContext.BaseDirectory, Fishy.Config.ReportFolder, DateTime.Now.ToString("ddMMyyyyHHmmss") + "issueReport.txt");
            string issueReport = "Issue Report\n" + String.Join(" ", arguments[1..]);
            File.WriteAllText(issuePath, issueReport);
            new MessagePacket("Your issues has been received and will be looked at as soon as possible.", "b30000").SendPacket("single", (int)CHANNELS.GAME_STATE, from);
            
        }
    }

}
