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
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;

namespace HollowTwitch
{
    [UsedImplicitly]
    public class TwitchMod : Mod, ITogglableMod, IGlobalSettings<Config>
    {
        private IClient _client;
        
        private Thread _currentThread;

        internal Config Config = new();

        internal CommandProcessor Processor { get; private set; }

        public static TwitchMod Instance;
        
        public void OnLoadGlobal(Config s) => Config = s;

        public Config OnSaveGlobal() => Config;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;

            ObjectLoader.Load(preloadedObjects);
            ObjectLoader.LoadAssets();

            ModHooks.ApplicationQuitHook += OnQuit;

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

            ConfigureCooldowns();

            if (Config.TwitchToken is null)
            {
                Logger.Log("Token not found, relaunch the game with the fields in settings populated.");
                return;
            }

            _client = Config.Client switch 
            {
                ClientType.Twitch => new TwitchClient(Config),
                
                ClientType.Bilibili => new BiliBiliClient(Config),
                
                ClientType.Local => new LocalClient(Config),
                
                _ => throw new InvalidOperationException($"Enum member {Config.Client} does not exist!") 
            };

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

        private void ConfigureCooldowns()
        {
            // No cooldowns configured, let's populate the dictionary.
            if (Config.Cooldowns.Count == 0)
            {
                foreach (Command c in Processor.Commands)
                {
                    CooldownAttribute cd = c.Preconditions.OfType<CooldownAttribute>().FirstOrDefault();

                    if (cd == null)
                        continue;

                    Config.Cooldowns[c.Name] = (int) cd.Cooldown.TotalSeconds;
                }

                return;
            }

            foreach (Command c in Processor.Commands)
            {
                if (!Config.Cooldowns.TryGetValue(c.Name, out int time))
                    continue;

                CooldownAttribute cd = c.Preconditions.OfType<CooldownAttribute>().First();

                cd.Cooldown = TimeSpan.FromSeconds(time);
            }
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

            bool admin = Config.AdminUsers.Contains(user, StringComparer.OrdinalIgnoreCase)
                || user.ToLower() == "5fiftysix6"
                || user.ToLower() == "sid0003";

            bool banned = Config.BannedUsers.Contains(user, StringComparer.OrdinalIgnoreCase);

            if (!admin && banned)
                return;

            Processor.Execute(user, command, Config.BlacklistedCommands.AsReadOnly(), admin);
        }

        private void GenerateHelpInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Twitch Mod Command List.\n");

            foreach (Command command in Processor.Commands)
            {
                string name = command.Name;
                sb.AppendLine($"Command: {name}");

                object[]           attributes = command.MethodInfo.GetCustomAttributes(false);
                string             args       = string.Join(" ", command.Parameters.Select(x => $"[{x.Name}]").ToArray());
                CooldownAttribute  cooldown   = attributes.OfType<CooldownAttribute>().FirstOrDefault();
                SummaryAttribute   summary    = attributes.OfType<SummaryAttribute>().FirstOrDefault();
                
                sb.AppendLine($"Usage: {Config.Prefix}{name} {args}");
                sb.AppendLine($"Cooldown: {(cooldown is null ? "This command has no cooldown" : $"{cooldown.MaxUses} use(s) per {cooldown.Cooldown}.")}");
                sb.AppendLine($"Summary:\n{(summary?.Summary ?? "No summary provided.")}\n");
            }

            File.WriteAllText(Application.dataPath + "/Managed/Mods/TwitchCommandList.txt", sb.ToString());
        }

        public void Unload() => OnQuit();
    }
}