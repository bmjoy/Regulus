﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regulus.Project.UnboundarySnake
{
    interface IStorage
    {
        Account? FindAccount(string account);
    }
}
