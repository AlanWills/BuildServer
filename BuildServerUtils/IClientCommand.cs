using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    public interface IClientCommand
    {
        string Description { get; }

        void Execute(BaseClient client, List<string> parameters);
    }
}
