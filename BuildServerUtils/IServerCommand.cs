using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    public interface IServerCommand
    {
        void Execute(BaseServer baseServer, string arguments);
    }
}
