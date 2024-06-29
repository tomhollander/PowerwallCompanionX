using PowerwallCompanion.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PowerwallCompanionX
{
    internal class MauiPlatformAdapter : IPlatformAdapter
    {
        public string AccessToken { get => Settings.AccessToken; set => Settings.AccessToken = value; }
        public string RefreshToken { get => Settings.RefreshToken; set => Settings.RefreshToken = value; }

        public async Task<string> ReadFileContents(string filename)
        {
            using (Stream stream = await FileSystem.Current.OpenAppPackageFileAsync(filename)) 
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public Task<JsonObject> ReadGatewayDetailsFromCache()
        {
            throw new NotImplementedException();
        }

        public Task SaveGatewayDetailsToCache(JsonObject json)
        {
            throw new NotImplementedException();
        }

        private static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName().Name;

            var stream = assembly.GetManifestResourceStream($"{assemblyName}.{filename}");

            return stream;
        }

        public string GetPersistedData(string key)
        {
            return Preferences.Default.Get<string>(key, null);
        }

        public void PersistData(string key, string value)
        {
            Preferences.Default.Set(key, value);
        }
    }
}
