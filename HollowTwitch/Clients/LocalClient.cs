using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HollowTwitch.Clients
{
    /// <summary>
    /// This is just for local testing
    /// You should make a server on your Machine and start it.
    /// The client will try to connect to your local Server and receive messages.
    /// </summary>
    internal class LocalClient : IClient
    {
        private const string LocalUser = "LocalUser";

        public event Action<string, string> ChatMessageReceived;
        public event Action<string> ClientErrored;

        private static TcpClient _client;
        private static byte[] receiveBuf;
        private static NetworkStream stream;

        private readonly int Port;
        
        public LocalClient(Config config,int port = 1234)
        {
            Port = port;
            
            config.AdminUsers.Add(LocalUser);
        }

        public void Dispose()
        {
            stream.Dispose();
            _client.Close();
        }

        public void StartReceive()
        {
            Connect("127.0.0.1", Port);
        }

        private void Connect(string host, int port)
        {
            _client = new TcpClient
            {
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096
            };

            receiveBuf = new byte[4096];
            
            Logger.Log("Connecting...");
            
            _client.BeginConnect(host, port, ConnectCallback, _client);
        }
        
        private void ConnectCallback(IAsyncResult result)
        {
            _client.EndConnect(result);

            if (!_client.Connected)
            {
                Logger.LogError("Connection failed.");
                
                return;
            }

            Logger.Log("Connection Successful. Waiting for messages.");

            stream = _client.GetStream();

            stream.BeginRead(receiveBuf, 0, 4096, RecvCallback, null);
        }
        
        private void RecvCallback(IAsyncResult result)
        {

            int byte_len = stream.EndRead(result);
            
            if (byte_len <= 0)
            {
                Logger.LogError("Received length < 0!");
                
                ClientErrored?.Invoke("Invalid length!");
                    
                return;
            }
            
            var data = new byte[byte_len];
            
            Array.Copy(receiveBuf, data, byte_len);

            string msg = Encoding.UTF8.GetString(data);
            
            Logger.Log($"Received message: {LocalUser}: {msg}");

            ChatMessageReceived?.Invoke(LocalUser, msg);
            
            stream.BeginRead(receiveBuf, 0, 4096, RecvCallback, null);
        }
    }
}
