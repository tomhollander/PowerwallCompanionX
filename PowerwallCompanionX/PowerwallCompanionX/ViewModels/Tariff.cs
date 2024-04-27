using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;
using static Android.Provider.CalendarContract;
using Xamarin.Forms;

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
                        c = Color.Blue;
                        break;
                    case "OFF_PEAK":
                        c = Color.Green;
                        break;
                    case "PARTIAL_PEAK":
                        c = Color.DarkOrange;
                        break;
                    case "ON_PEAK":
                        c = Color.Red;
                        break;
                    default:
                        c = Color.DarkGray;
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
