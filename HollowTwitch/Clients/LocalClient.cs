using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HollowTwitch.Clients
{
    class LocalClient : IClient
    {
        public event Action<string, string> ChatMessageReceived;
        public event Action<string> ClientErrored;

        private static TcpClient _client;
        private static byte[] receiveBuf;
        private static NetworkStream stream;
        public void Dispose()
        {
            stream.Dispose();
            _client.Close();
        }

        public void StartReceive()
        {
            Connect("127.0.0.1", 1234);
        }

        public void Connect(string host, int port)
        {
            _client = new TcpClient
            {
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096
            };

            receiveBuf = new byte[4096];
            Log("Connecting...");
            _client.BeginConnect(host, port, ConnectCallback, _client);

        }
        private void ConnectCallback(IAsyncResult result)
        {
            _client.EndConnect(result);

            if (!_client.Connected)
            {
                Log("Connect Failed");
                return;
            }

            Log("Connect Success,Waiting for Msg");

            stream = _client.GetStream();

            stream.BeginRead(receiveBuf, 0, 4096, RecvCallback, null);
        }
        private void RecvCallback(IAsyncResult result)
        {

            int byte_len = stream.EndRead(result);
            if (byte_len <= 0)
            {
                Log("Length Error");
                return;
            }
            byte[] data = new byte[byte_len];
            Array.Copy(receiveBuf, data, byte_len);

            string msg = System.Text.Encoding.UTF8.GetString(data);
            Log("a2659802:"+msg);

            ChatMessageReceived.Invoke("a2659802", msg);
            stream.BeginRead(receiveBuf, 0, 4096, RecvCallback, null);
        }
        static void Log(object msg) => Modding.Logger.LogDebug(msg);
    }
}
