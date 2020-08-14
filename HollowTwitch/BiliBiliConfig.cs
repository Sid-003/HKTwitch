using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch
{
    // if your bilibili streamming url is like this 'https://live.bilibili.com/22102251?visit_id=7t294f4lbb40'
    // so 22102251 is your room id
    [Serializable]
    class BiliBiliConfig : TwitchConfig
    {
        public new string Token { get => "Not Need Token"; }
        public new string Channel
        {
            get
            {
                int _room_id;
                if (base.Channel != null)
                {
                    try
                    {
                        _room_id = Convert.ToInt32(base.Channel);
                    }
                    catch
                    {
                        _room_id = -1;
                    }
                }
                else
                {
                    _room_id = -2;
                }
                return _room_id.ToString();
            }
            set
            {
                base.Channel = value;
            }
            //set => _room_id = Convert.ToInt32(value); 
        }
    
        public BiliBiliConfig()
        {
            if(base.Token is null)
            {
                base.Token = Token;
            }
        }
    }
}
