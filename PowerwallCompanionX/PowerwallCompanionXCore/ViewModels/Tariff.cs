using System.Globalization;

namespace PowerwallCompanionX.ViewModels
{
    public class Tariff
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Season { get; set; }
        public string DisplayName
        {
            get
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                return textInfo.ToTitleCase(Name.ToLower().Replace("_", " "));
            }
        }
        public Color Color
        {
            get
            {
                Color c;
                switch (Name)
                {
                    case "SUPER_OFF_PEAK":
                        c = Colors.Blue;
                        break;
                    case "OFF_PEAK":
                        c = Colors.Green;
                        break;
                    case "PARTIAL_PEAK":
                        c = Colors.DarkOrange;
                        break;
                    case "ON_PEAK":
                        c = Colors.Red;
                        break;
                    default:
                        c = Colors.DarkGray;
                        break;
                }
                return c;
            }
        }

        public override bool Equals(object obj)
        {
            var t = obj as Tariff;
            if (t == null)
                return false;
            return t.Name == Name && t.StartDate == StartDate && t.EndDate == EndDate;
        }

    }
}
