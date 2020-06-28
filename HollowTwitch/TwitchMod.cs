using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HollowTwitch.Commands;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using Modding;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;

namespace HollowTwitch
{
    public class TwitchMod : Mod
    {
        private TwitchClient _client;
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
            Processor.RegisterCommands<Meta>();

            _client = new TwitchClient(Config);
            _client.ChatMessageReceived += OnMessageReceived;

            _client.ClientErrored += s => Log($"An error occured while receiving messages.\nError: {s}");

            _currentThread = new Thread(_client.StartReceive)
            {
                IsBackground = true
            };
            _currentThread.Start();
            
            #if DEBUG
            GenerateHelpInfo();
            #endif
            
            Log("Started receiving");

            once = true;
        }

        private void OnQuit()
        {
            _currentThread.Abort();
            _client.Dispose();
        }

        private void OnMessageReceived(string user, string message)
        {
            Log($"Twitch chat: [{user}: {message}]");

            string trimmed = message.Trim();
            int index = trimmed.IndexOf(Config.Prefix);

            if (index != 0) return;

            string command = trimmed.Substring(Config.Prefix.Length).Trim();

            Processor.Execute(user, command, Config);
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
    }
}