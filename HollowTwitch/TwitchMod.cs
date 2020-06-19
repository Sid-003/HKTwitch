using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HollowTwitch.Commands;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using JetBrains.Annotations;
using Modding;
using On.HutongGames.PlayMaker.Actions;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;
using Logger = Modding.Logger;

namespace HollowTwitch
{
    public class TwitchMod : Mod
    {
        private TwitchClient _client;
        private Thread _currentThread;
        private TwitchConfig _config = new TwitchConfig();

        public CommandProcessor Processor;

        public static TwitchMod Instance;
        public override ModSettings GlobalSettings
        {
            get => _config;
            set => _config = value as TwitchConfig;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            ObjectLoader.Load(preloadedObjects);
            ObjectLoader.LoadAssets();
            
            ModHooks.Instance.AfterSavegameLoadHook += OnSaveGameLoad;
            ModHooks.Instance.NewGameHook += OnNewGame;
            ModHooks.Instance.ApplicationQuitHook += OnQuit;
        }

        public override List<(string, string)> GetPreloadNames() => ObjectLoader.ObjectList.Values.ToList();

        private void OnSaveGameLoad(SaveGameData data) => ReceiveCommands();

        private void OnNewGame() => ReceiveCommands();

        private static bool once;

        private void ReceiveCommands()
        {
            if (once) return;

            Processor = new CommandProcessor();
            Processor.RegisterCommands<Player>();
            Processor.RegisterCommands<Enemies>();
            Processor.RegisterCommands<Area>();
            Processor.RegisterCommands<Camera>();
            Processor.RegisterCommands<Game>();
            _client = new TwitchClient(_config);
            _client.ChatMessageReceived += OnMessageReceived;
            _client.ClientErrored += s =>
            {
                Logger.Log($"An error occured while receiving messages.\nError: " + s);
            };
            _currentThread = new Thread(_client.StartReceive);
            _currentThread.Start();
            #if DEBUG
                GenerateHelpInfo();
            #endif
            Logger.Log("started receiving");
            once = true;
        }

        private void OnQuit()
        {
            _currentThread.Abort();
            _client.Dispose();
        }

        private void OnMessageReceived(string message)
        {
            Logger.Log("Twitch chat: " + message);
            
            string trimmed = message.Trim();
            int index = trimmed.IndexOf(_config.Prefix);

            if (index != 0) return;
            
            string command = trimmed.Substring(_config.Prefix.Length).Trim();
            
            Processor.Execute(command);
        }

        private void GenerateHelpInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Twitch Mod Command List.\n");
            
            foreach (Command command in Processor.Commands)
            {
                string name = command.Name;
                sb.AppendLine($"Command: {name}");

                var attributes = command.MethodInfo.GetCustomAttributes(false);
                var args = string.Join(" ", command.Parameters.Select(x => $"[{x.Name}]").ToArray());
                var cooldown = attributes.OfType<CooldownAttribute>().FirstOrDefault();
                var summary = attributes.OfType<SummaryAttribute>().FirstOrDefault();
                sb.AppendLine($"Usage: {_config.Prefix}{name} {args}");
                sb.AppendLine($"Cooldown: {(cooldown is null ? "This command has no cooldown" : $"{cooldown.MaxUses} use(s) per {cooldown.Reset}.")}");
                sb.AppendLine($"Summary:\n{(summary?.Summary ?? "No summary provided.")}\n");
            }

            File.WriteAllText(Application.dataPath + "/Managed/Mods/TwitchCommandList.txt", sb.ToString());
        }
        
    }
}