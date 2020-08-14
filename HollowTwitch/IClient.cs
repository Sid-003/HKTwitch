using System;

namespace HollowTwitch
{
    public interface IClient : IDisposable
    {
        event Action<string, string> ChatMessageReceived;

        event Action<string> ClientErrored;

        void StartReceive();
    }
}
