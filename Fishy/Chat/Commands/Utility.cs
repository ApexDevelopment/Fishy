using Fishy.Models;
using Fishy.Models.Packets;
using Fishy.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishy.Chat.Commands
{
	internal class TeleportCommand : Command
	{
		public override string Name => "tp";
		public override string Description => "Teleport to a player";
		public override PermissionLevel PermissionLevel => PermissionLevel.Admin;
		public override string[] Aliases => [];
		public override string Help => "!tp player";

		public override void OnUse(SteamId executor, string[] arguments)
		{
			if (arguments.Length == 0) { ChatUtils.SendChat(executor, Help); return; }

			Player? playerToTp = ChatUtils.FindPlayer(executor);
			Player? playerToTpTo = ChatUtils.FindPlayer(arguments[0]);

			if (playerToTp == null)
			{
				ChatUtils.SendChat(executor, "!tp cannot be executed from the console.");
				return;
			}

			if (playerToTpTo == null)
			{
				ChatUtils.SendChat(executor, $"Cannot find a player with the name or ID {arguments[0]}.");
				return;
			}

			// TODO: Why is Player.InstanceID a long, but Actor.InstanceID an int?
			new ActorUpdatePacket((int)playerToTp.InstanceID, playerToTpTo.Position, playerToTpTo.Rotation).SendPacket("all", (int)CHANNELS.ACTOR_UPDATE);

			ChatUtils.SendChat(executor, $"Teleported you to {playerToTpTo.Name}");
		}
	}
}
