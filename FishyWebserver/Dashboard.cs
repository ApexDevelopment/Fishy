﻿using System.Text.Json;
using Fishy.Models;
using Fishy.Models.Packets;
using Fishy.Utils;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.ServerSentEvents;

namespace Fishy.Webserver
{
    internal class Dashboard
    {
        internal readonly List<ChatMessage> MessageToSync = [];
        internal readonly Dictionary<Player, string> PlayersToSync = [];
        public void Initalize()
        {
            var mainPage = Content.From(Resource.FromAssembly("index.html"));
            var syncEvent = EventSource.Create().Generator(SyncStats);

            var app = Layout.Create()
                .Index(mainPage)
                .Add("status", syncEvent)
                .AddController<ActionController>("action")
                .Authentication(Authentication.Auth);

            Host.Create()
                .Defaults()
                .Handler(app)
                .StartAsync();
        }


        async ValueTask SyncStats(IEventConnection connection)
        {
            // Resync data on reload
            foreach (Player p in WebserverExtension.Players)
                await connection.DataAsync(JsonSerializer.Serialize(p), "join");

            foreach (ChatMessage m in WebserverExtension.ChatLog)
                await connection.DataAsync(m.ToString(), "message");

            // Sync new data while connected
            while (connection.Connected)
            {
                await connection.DataAsync(WebserverExtension.Players.Count, "players");
                await connection.DataAsync(WebserverExtension.BannedPlayers.Count, "banned");
                await connection.DataAsync(WebserverExtension.Actors.Count, "actors");

                if (MessageToSync.Count > 0)
                {
                    foreach (ChatMessage s in MessageToSync)
                        await connection.DataAsync(s.ToString(), "message");
                    
                    MessageToSync.Clear();
                }

                if (PlayersToSync.Count > 0)
                {
                    foreach (KeyValuePair<Player, string> p in PlayersToSync)
                        await connection.DataAsync(JsonSerializer.Serialize(p.Key), p.Value);

                    PlayersToSync.Clear();
                }

                await Task.Delay(100);
            }
        }
    }
}
