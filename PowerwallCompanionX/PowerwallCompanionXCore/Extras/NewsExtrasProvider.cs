using System.Xml.Linq;

namespace PowerwallCompanionX.Extras
{
    internal class NewsExtrasProvider : IExtrasProvider
    {
        private string _feedUrl;
        private List<string> _headlines;
        private int _nextIndex;
        private int _timesDisplayed;
        public NewsExtrasProvider(string feedUrl)
        {
            Telemetry.TrackEvent("NewsExtrasProvider initialised");
            _feedUrl = feedUrl;
        }

        public async Task<string> RefreshStatus()
        {
            try
            {

                if (_headlines == null || _nextIndex >= _headlines.Count)
                {
                    await LoadFeed();
                    _nextIndex = 0;
                    _timesDisplayed = 0;
                }

                string message = "🗞️" + _headlines[_nextIndex];

                // Move to next headline every 2 refreshes
                _timesDisplayed = (_timesDisplayed + 1) % 2;
                if (_timesDisplayed == 0)
                {
                    _nextIndex++;
                }
                return message;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                return "News RSS failed";
            } 
        }

        private async Task LoadFeed()
        {
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(_feedUrl))
            { 
                var doc = await XDocument.LoadAsync(stream, LoadOptions.None, default);
                var root = doc.Root;
                var channel = root.Element("channel");
                var items = channel.Elements("item");
                _headlines = new List<string>();

                foreach (XElement item in items)
                {
                    var title = item.Element("title");
                    _headlines.Add(title.Value);
                }
            }
            
        }
    }
}

