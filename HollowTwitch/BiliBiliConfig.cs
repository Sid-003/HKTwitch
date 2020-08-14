using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch
{
    class BiliBiliConfig : TwitchConfig
    {
        private int _room_id;
        public new string Token { get => "Not Need Token"; }
        public new string Channel { get => _room_id.ToString(); set => _room_id = Convert.ToInt32(value); }
    }
}
