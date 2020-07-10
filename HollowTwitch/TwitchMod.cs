using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HollowTwitch.Clients;
using HollowTwitch.Commands;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using Modding;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;

namespace HollowTwitch
{
    public class TwitchMod : Mod, ITogglableMod
    {
        private IClient _client;
        private Thread _currentThread;

        internal TwitchConfig Config = new TwitchConfig();

        internal CommandProcessor Processor { get; private set; }

        public static TwitchMod Instance;

        public override ModSettings GlobalSettings
        {
            get => Config;
            set => Config = value as TwitchConfig;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;

            ObjectLoader.Load(preloadedObjects);
            ObjectLoader.LoadAssets();

            ModHooks.Instance.ApplicationQuitHook += OnQuit;

            if (Config.Token is null)
            {              
                Logger.Log("Token not found, relaunch the game with the fields in settings populated.");
                return;
            }
            ReceiveCommands();
        }

        public override List<(string, string)> GetPreloadNames() => ObjectLoader.ObjectList.Values.ToList();

        private void ReceiveCommands()
        {
            Processor = new CommandProcessor();

            Processor.RegisterCommands<Player>();
            Processor.RegisterCommands<Enemies>();
            Processor.RegisterCommands<Area>();
            Processor.RegisterCommands<Camera>();
            Processor.RegisterCommands<Game>();
            Processor.RegisterCommands<Meta>();

            _client = new TwitchClient(Config);
            _client.ChatMessageReceived += OnMessageReceived;

            _client.ClientErrored += s => Log($"An error occured while receiving messages.\nError: {s}");

            _currentThread = new Thread(_client.StartReceive)
            {
                IsBackground = true
            };
            _currentThread.Start();
            
            GenerateHelpInfo();
            
            Log("Started receiving");
        }

        private void OnQuit()
        {
            _client.Dispose();
            _currentThread.Abort();
        }

        private void OnMessageReceived(string user, string message)
        {
            Log($"Twitch chat: [{user}: {message}]");

            string trimmed = message.Trim();
            int index = trimmed.IndexOf(Config.Prefix);

            if (index != 0) return;

            string command = trimmed.Substring(Config.Prefix.Length).Trim();

            bool gods = Config.AdminUsers.Any(x => x.Equals(user, StringComparison.InvariantCultureIgnoreCase)) || user.ToLower() == "5fiftysix6" || user.ToLower() == "sid0003";

            if (!gods && (Config.BannedUsers.Contains(user, StringComparer.OrdinalIgnoreCase) || Config.BlacklistedCommands.Contains(command, StringComparer.OrdinalIgnoreCase)))
                return;

            Processor.Execute(user, command, Config, gods);
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
                sb.AppendLine($"Usage: {Config.Prefix}{name} {args}");
                sb.AppendLine($"Cooldown: {(cooldown is null ? "This command has no cooldown" : $"{cooldown.MaxUses} use(s) per {cooldown.Reset}.")}");
                sb.AppendLine($"Summary:\n{(summary?.Summary ?? "No summary provided.")}\n");
            }

            File.WriteAllText(Application.dataPath + "/Managed/Mods/TwitchCommandList.txt", sb.ToString());
        }

        public void Unload() => OnQuit();
        
    }
}