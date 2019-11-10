using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HollowTwitch
{
    class TwitchClient : IDisposable
    {
        private TcpClient _client;
        private StreamReader _output;
        private StreamWriter _input;

        private readonly TwitchConfig _config;

        public event Action<string> ChatMessageReceived;
        public event Action<string> RawPayload;

        public TwitchClient(TwitchConfig config)
        {
            _config = config;
            ConnectAndAuthenticate(config);
            this.RawPayload += ProcessMessage;
        }

        private void ConnectAndAuthenticate(TwitchConfig config)
        {
            _client = new TcpClient("irc.twitch.tv", 6667);

            _output = new StreamReader(_client.GetStream());
            _input = new StreamWriter(_client.GetStream())
            {
                AutoFlush = true
            };

            SendMessage($"PASS oauth:{config.Token}");
            SendMessage($"NICK {config.Username}");
            SendMessage($"JOIN #{config.Channel}");
        }

        private void ProcessMessage(string message)
        {
            if (message == null)
                return;

            if (message.Contains("PING"))
            {
                SendMessage("PONG :tmi.twitch.tv");
                Console.WriteLine("sent pong!");
            }
            else if (message.Contains("PRIVMSG"))
            {
                var cleaned = message.Split(':').Last();
                ChatMessageReceived?.Invoke(cleaned);
            }
        }

        public void StartReceive()
        {
            while (true)
            {
                if (!_client.Connected)
                {
                    Dispose();
                    ConnectAndAuthenticate(_config);
                }
                var message = _output.ReadLine();
                RawPayload?.Invoke(message);
            }
        }


        private void SendMessage(string message)
            => _input.WriteLine(message);

        public void Dispose()
        {
            _input.Dispose();
            _output.Dispose();
            _client.Close();
        }
    }
}
