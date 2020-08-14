using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace HollowTwitch.Clients
{
    internal class TwitchClient : IClient
    {
        private TcpClient _client;
        private StreamReader _output;
        private StreamWriter _input;

        private readonly Config _config;

        public event Action<string, string> ChatMessageReceived;
        public event Action<string> RawPayload;

        public event Action<string> ClientErrored;

        public TwitchClient(Config config)
        {
            _config = config;
            ConnectAndAuthenticate(config);
            RawPayload += ProcessMessage;
        }

        private void ConnectAndAuthenticate(Config config)
        {
            _client = new TcpClient("irc.twitch.tv", 6667);

            _output = new StreamReader(_client.GetStream());
            _input = new StreamWriter(_client.GetStream())
            {
                AutoFlush = true
            };

            if (!_client.Connected)
            {
                Reconnect(10000);
                return;
            }
                
            SendMessage($"PASS oauth:{config.TwitchToken}");
            SendMessage($"NICK {config.TwitchUsername}");
            SendMessage($"JOIN #{config.TwitchChannel}");
        }

        private void Reconnect(int delay)
        {
            ClientErrored?.Invoke("Reconnecting........");
            Dispose();
            Thread.Sleep(delay);
            ConnectAndAuthenticate(_config);
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
                string user = message.Substring(1, message.IndexOf("!") - 1);
                string cleaned = message.Split(':').Last();
                
                ChatMessageReceived?.Invoke(user, cleaned);
            }
        }

        public void StartReceive()
        {
            while (true)
            {
                try
                {
                    if (!_client.Connected)
                    {
                        Dispose();
                        ConnectAndAuthenticate(_config);
                    }

                    string message = _output.ReadLine();
                    RawPayload?.Invoke(message);
                }
                catch (Exception e)
                {
                    ClientErrored?.Invoke("Error occured trying to read stream: " + e);
                    Reconnect(5000);
                }
               
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void SendMessage(string message) => _input.WriteLine(message);

        public void Dispose()
        {
            _input.Dispose();
            _output.Dispose();
            _client.Close();
        }
    }
}