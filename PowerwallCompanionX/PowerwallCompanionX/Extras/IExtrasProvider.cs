using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerwallCompanionX.Extras
{
    internal interface IExtrasProvider
    {
        Task<string> RefreshStatus();
    }
}
