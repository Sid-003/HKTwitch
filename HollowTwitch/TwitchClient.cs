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
        private readonly StreamReader _output;
        private readonly StreamWriter _input;

        public event Action<string> ChatMessageReceived;
        public event Action<string> RawPayload;
        public TwitchClient(string oauth, string username, string channel)
        {
            _client = new TcpClient("irc.twitch.tv", 6667);
            
            _output = new StreamReader(_client.GetStream());
            _input = new StreamWriter(_client.GetStream())
            {
                AutoFlush = true
            };

            SendMessage($"PASS oauth:{oauth}");
            SendMessage($"NICK {username}");
            SendMessage($"JOIN #{channel}");
          
            this.RawPayload += ProcessMessage;
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
            if (!_client.Connected)
            {
                _client.Close();
                _client = new TcpClient("irc://irc-ws.chat.twitch.tv", 6667);
            }
            while (true)
            {
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
