using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    public class CommandAttribute : Attribute
    {
        public string Token { get; private set; }

        public CommandAttribute(string token)
        {
            Token = token;
        }
    }
}
