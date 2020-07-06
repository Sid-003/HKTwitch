using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch
{
    public interface IClient : IDisposable
    {
        event Action<string, string> ChatMessageReceived;

        event Action<string> ClientErrored;

        void StartReceive();
    }
}
