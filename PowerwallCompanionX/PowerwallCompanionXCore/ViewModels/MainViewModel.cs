using PowerwallCompanion.Lib.Models;
using PowerwallCompanionX.Views;
using System.ComponentModel;
using System.Globalization;

namespace PowerwallCompanionX.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public enum StatusEnum
        {
            Online,
            GridOutage,
            Error
        }

        
        private DateTime _batteryDay = DateTime.MinValue;
        private StatusEnum _status;
        public string _extrasContent; 

        public MainViewModel()
        {
            SettingsCommand = new Command(OnSettingsTapped);
        }

        private void OnSettingsTapped(object obj)
        {
            Application.Current.MainPage = new SettingsPage();
        }


        public void NotifyPowerProperties()
        {
            NotifyPropertyChanged(nameof(InstantaneousPower));
            NotifyPropertyChanged(nameof(Time));
            NotifyPropertyChanged(nameof(PageOpacity));
        }

        public void NotifyDailyEnergyProperties()
        { 
            NotifyPropertyChanged(nameof(EnergyTotalsToday));
            NotifyPropertyChanged(nameof(EnergyTotalsYesterday));
            NotifyPropertyChanged(nameof(ShowBothGridSettingsToday));
            NotifyPropertyChanged(nameof(ShowBothGridSettingsYesterday));
            NotifyPropertyChanged(nameof(ShowSingleGridSettingsToday));
            NotifyPropertyChanged(nameof(ShowSingleGridSettingsYesterday));
            NotifyPropertyChanged(nameof(Time));
            NotifyPropertyChanged(nameof(PageOpacity));
        }

        public void NotifyGraphProperties()
        {
            NotifyPropertyChanged(nameof(PowerChartSeries));
            NotifyPropertyChanged(nameof(GraphDayBoundary));
            NotifyPropertyChanged(nameof(ChartMaxDate));
        }

        public void NotifyChangedSettings()
        {
            NotifyPropertyChanged(nameof(ShowClock));
            NotifyPropertyChanged(nameof(ShowEnergyCosts));
        }

        public InstantaneousPower InstantaneousPower
        {
            get; set;
        }



        public double MinBatteryPercentToday
        {
            get; set;
        }

        public double MaxBatteryPercentToday
        {
            get; set;
        }


        public EnergyTotals EnergyTotalsYesterday
        {
            get; set;
        }

        public EnergyTotals EnergyTotalsToday
        {
            get; set;
        }



        public bool ShowBothGridSettingsToday
        {
            get => EnergyTotalsToday == null ? false : EnergyTotalsToday.GridEnergyExported > 500;  
        }

        public bool ShowSingleGridSettingsToday
        {
            get => !ShowBothGridSettingsToday;
        }

        public bool ShowBothGridSettingsYesterday
        {
            get => EnergyTotalsYesterday == null ? false : EnergyTotalsYesterday.GridEnergyExported > 500; 
        }

        public bool ShowSingleGridSettingsYesterday
        {
            get => !ShowBothGridSettingsYesterday;
        }

        public PowerChartSeries PowerChartSeries
        {
            get; set;
        }

        public DateTime ChartMaxDate
        {
            get; set;
        }

        public StatusEnum Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged(nameof(Status));
            }
        }

        public DateTime BatteryDay
        {
            get; set;
        }

        public string ExtrasContent
        {
            get { return _extrasContent; }
            set
            {
                _extrasContent = value;
                NotifyPropertyChanged(nameof(ExtrasContent));
            }
        }

        public string Time
        {
            get
            {
#if ANDROID
                bool is24Hour = Android.Text.Format.DateFormat.Is24HourFormat(Android.App.Application.Context);
                if (is24Hour)
                {
                    return DateTime.Now.ToString("HH:mm");
                }
                else
                {
                    return DateTime.Now.ToString("h:mm");
                }
#else
                
                string pattern = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
                string patternWithoutAmPm = pattern.Replace("tt", "");
                return DateTime.Now.ToString(patternWithoutAmPm);
#endif
            }
        }

        public bool ShowClock
        {
            get
            {
                return Settings.ShowClock;
            }
        }

        public bool ShowEnergyCosts
        {
            get
            {
                return Settings.ShowEnergyCosts;
            }
        }

        public double LargeFontSize
        {
            get { return Settings.FontScale * 55; }
        }
        public double MediumFontSize
        {
            get { return Settings.FontScale * 40; }
        }

        public double SmallFontSize
        {
            get { return Settings.FontScale * 30; }
        }

        public double LargeCaptionFontSize
        {
            get { return Settings.FontScale * 20; }
        }

        public double SmallCaptionFontSize
        {
            get { return Settings.FontScale * 16; }
        }

        public Thickness BigNumberMargin
        {
            get { return new Thickness(0,0,5, -Settings.FontScale * 15 ); }
        }

        public bool ShowGraph
        {
            get => Settings.ShowGraph;
        }
        public Command SettingsCommand { get; }

        public string LastExceptionMessage { get; set; }
        public DateTime LastExceptionDate { get; set; }
        public DateTime LiveStatusLastRefreshed { get; set;  }
        public DateTime EnergyHistoryLastRefreshed { get; set; }

        public DateTime PowerHistoryLastRefreshed { get; set; }
        public DateTime GraphDayBoundary
        {
            get { return DateTime.Today; }
        }


        

        public double PageWidth { get; set; }
        public double PageHeight { get; set; }

        public void RecalculatePageMargin()
        {
            if (Settings.PreventBurnIn)
            {
                double horizontalMarginTotal = PageWidth > 700 ? PageWidth * 0.05 : 0;
                double verticalMarginTotal = PageHeight > 700 ? PageHeight * 0.05 : 0;

                var random = new Random();
                var leftMargin = random.NextDouble() * horizontalMarginTotal;
                var rightMargin = horizontalMarginTotal - leftMargin;
                var topMargin = random.NextDouble() * verticalMarginTotal;
                var bottomMargin = verticalMarginTotal - topMargin;
                PageMargin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
                NotifyPropertyChanged(nameof(PageMargin));
            }
            else
            {
                PageMargin = new Thickness(0);
            }

        }

        public Thickness PageMargin
        {
            get; set;
        }

        public double PageOpacity
        {
            get
            {
                if (Settings.DimAtNight && (DateTime.Now.Hour < 6 || DateTime.Now.Hour >= 22))
                {
                    return 0.3;
                }
                else
                {
                    return 1.0;
                }
            }
        }

        public decimal EnergyCostYesterday { get; set; }
        public decimal EnergyCostToday { get; set; }
        public decimal EnergyFeedInYesterday { get; set; }
        public decimal EnergyFeedInToday { get; set; }
        public decimal EnergyNetCostYesterday
        {
            get => EnergyCostYesterday - EnergyFeedInYesterday;
        }

        public decimal EnergyNetCostToday
        {
            get => EnergyCostToday - EnergyFeedInToday;
        }

        public void NotifyEnergyCostProperties()
        {
            NotifyPropertyChanged(nameof(EnergyCostYesterday));
            NotifyPropertyChanged(nameof(EnergyCostToday));
            NotifyPropertyChanged(nameof(EnergyFeedInYesterday));
            NotifyPropertyChanged(nameof(EnergyFeedInToday));
            NotifyPropertyChanged(nameof(EnergyNetCostYesterday));
            NotifyPropertyChanged(nameof(EnergyNetCostToday));

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
