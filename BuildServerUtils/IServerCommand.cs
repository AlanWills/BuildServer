using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    public interface IServerCommand
    {
        string Execute(BaseServer baseServer, NameValueCollection arguments);
    }
}
