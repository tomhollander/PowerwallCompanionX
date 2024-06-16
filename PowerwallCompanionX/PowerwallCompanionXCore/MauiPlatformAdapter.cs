using PowerwallCompanion.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PowerwallCompanionX
{
    internal class MauiPlatformAdapter : IPlatformAdapter
    {
        public string AccessToken { get => Settings.AccessToken; set => Settings.AccessToken = value; }
        public string RefreshToken { get => Settings.RefreshToken; set => Settings.RefreshToken = value; }

        public Task<string> ReadFileContents(string filename)
        {
            throw new NotImplementedException();
        }

        public Task<JsonObject> ReadGatewayDetailsFromCache()
        {
            throw new NotImplementedException();
        }

        public Task SaveGatewayDetailsToCache(JsonObject json)
        {
            throw new NotImplementedException();
        }
    }
}
