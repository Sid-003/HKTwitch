using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Entities
{
    public interface IArgumentParser
    {
        object Parse(string arg);
    }
}
