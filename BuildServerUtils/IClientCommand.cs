﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    public interface IClientCommand
    {
        void Execute(BaseClient client);
    }
}
