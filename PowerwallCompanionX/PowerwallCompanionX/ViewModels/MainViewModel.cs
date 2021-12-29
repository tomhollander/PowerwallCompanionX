﻿using PowerwallCompanionX.Views;
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public enum StatusEnum
        {
            IdleGrid,
            ImportingFromGrid,
            ExportingToGrid,
            Error
        }

        private double _batteryPercent;
        private double _homeValue;
        private double _solarValue;
        private double _batteryValue;
        private double _gridValue;
        private bool _gridActive;
        private StatusEnum _status;

        public MainViewModel()
        {
            SettingsCommand = new Command(OnSettingsTapped);
        }

        private void OnSettingsTapped(object obj)
        {
            Application.Current.MainPage = new SettingsPage();
        }


        public void NotifyProperties()
        {
            NotifyPropertyChanged(nameof(BatteryPercent));
            NotifyPropertyChanged(nameof(BatteryStatus));
            NotifyPropertyChanged(nameof(HomeValue));
            NotifyPropertyChanged(nameof(HomeFromBattery));
            NotifyPropertyChanged(nameof(HomeFromGrid));
            NotifyPropertyChanged(nameof(HomeFromSolar));
            NotifyPropertyChanged(nameof(SolarValue));
            NotifyPropertyChanged(nameof(SolarToBattery));
            NotifyPropertyChanged(nameof(SolarToGrid));
            NotifyPropertyChanged(nameof(SolarToHome));
            NotifyPropertyChanged(nameof(GridValue));
            NotifyPropertyChanged(nameof(GridActive));
            NotifyPropertyChanged(nameof(HomeEnergyYesterday));
            NotifyPropertyChanged(nameof(HomeEnergyToday));
            NotifyPropertyChanged(nameof(SolarEnergyYesterday));
            NotifyPropertyChanged(nameof(SolarEnergyToday));
            NotifyPropertyChanged(nameof(GridEnergyImportedYesterday));
            NotifyPropertyChanged(nameof(GridEnergyImportedToday));
            NotifyPropertyChanged(nameof(GridEnergyExportedYesterday));
            NotifyPropertyChanged(nameof(GridEnergyExportedToday));
            NotifyPropertyChanged(nameof(BatteryEnergyImportedYesterday));
            NotifyPropertyChanged(nameof(BatteryEnergyImportedToday));
            NotifyPropertyChanged(nameof(BatteryEnergyExportedYesterday));
            NotifyPropertyChanged(nameof(BatteryEnergyExportedToday));
            NotifyPropertyChanged(nameof(ShowBothGridSettingsToday));
            NotifyPropertyChanged(nameof(ShowBothGridSettingsYesterday));
            NotifyPropertyChanged(nameof(Time));
        }

        public void NotifyGraphProperties()
        {
            NotifyPropertyChanged(nameof(HomeGraphData));
            NotifyPropertyChanged(nameof(SolarGraphData));
            NotifyPropertyChanged(nameof(BatteryGraphData));
            NotifyPropertyChanged(nameof(GridGraphData));
            NotifyPropertyChanged(nameof(GraphDayBoundary));
            NotifyPropertyChanged(nameof(ChartMaxDate));
        }
        public void NotifyChangedSettings()
        {
            NotifyPropertyChanged(nameof(ShowClock));
        }

        public double BatteryPercent
        {
            get { return _batteryPercent; }
            set
            {
                _batteryPercent = value;
            }
        }
        public double HomeValue
        {
            get { return _homeValue; }
            set
            {
                _homeValue = value;
            }
        }

        public double SolarValue
        {
            get { return _solarValue; }
            set
            {
                _solarValue = value;

            }
        }

        public double BatteryValue
        {
            get { return _batteryValue; }
            set
            {
                _batteryValue = value;
            }
        }

        public double GridValue
        {
            get { return _gridValue; }
            set
            {
                _gridValue = value;
            }
        }

        public double HomeEnergyToday
        {
            get; set; 
        }

        public double HomeEnergyYesterday
        {
            get; set;
        }

        public double SolarEnergyToday
        {
            get; set;
        }

        public double SolarEnergyYesterday
        {
            get; set;
        }

        public double GridEnergyImportedToday
        {
            get; set;
        }

        public double GridEnergyImportedYesterday
        {
            get; set;
        }

        public double GridEnergyExportedToday
        {
            get; set;
        }

        public double GridEnergyExportedYesterday
        {
            get; set;
        }

        public bool ShowBothGridSettingsToday
        {
            get { return GridEnergyExportedToday > 500;  }
        }

        public bool ShowBothGridSettingsYesterday
        {
            get { return GridEnergyExportedYesterday > 500;  }
        }

        public double BatteryEnergyImportedToday
        {
            get; set;
        }

        public double BatteryEnergyImportedYesterday
        {
            get; set;
        }

        public double BatteryEnergyExportedToday
        {
            get; set;
        }

        public double BatteryEnergyExportedYesterday
        {
            get; set;
        }

        public double HomeFromGrid
        {
            get { return GridValue > 0D ? GridValue : 0D; }
        }

        public double HomeFromBattery
        {
            get { return BatteryValue > 0D ? BatteryValue : 0D; }
        }

        public double HomeFromSolar
        {
            get { return HomeValue - HomeFromGrid - HomeFromBattery; }
        }

        public double SolarToGrid
        {
            get { return GridValue < 0D ? -GridValue : 0D; }
        }

        public double SolarToBattery
        {
            get { return BatteryValue < 0D ? -BatteryValue : 0D; }
        }

        public double SolarToHome
        {
            get { return SolarValue - SolarToGrid - SolarToBattery; }
        }

        public List<ChartDataPoint> HomeGraphData
        {
            get; set;
        }
        public List<ChartDataPoint> SolarGraphData
        {
            get; set;
        }
        public List<ChartDataPoint> GridGraphData
        {
            get; set;
        }
        public List<ChartDataPoint> BatteryGraphData
        {
            get; set;
        }

        public DateTime ChartMaxDate
        {
            get
            {
                if (Settings.AccessToken == "DEMO")
                {
                    return new DateTime(2021, 04, 17); // Match the dummy data
                }
                return DateTime.Today.AddDays(1);
            }
        }
        public StatusEnum Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged(nameof(Status));
                NotifyPropertyChanged(nameof(StatusString));
            }
        }

        public string StatusString
        {
            get
            {
                switch (Status)
                {
                    case StatusEnum.Error:
                        return "Connection Error";
                    case StatusEnum.ExportingToGrid:
                        return "Exporting to Grid";
                    case StatusEnum.ImportingFromGrid:
                        return "Importing from Grid";
                    case StatusEnum.IdleGrid:
                        return "Grid Idle";
                    default:
                        return null;
                }
            }
        }


        public string BatteryStatus
        {
            get
            {
                if (BatteryValue < -20)
                {
                    return "Charging";
                }
                else if (BatteryValue > 20)
                {
                    return "Discharging";
                }
                else
                {
                    return "Standby";
                }
            }
        }

        public bool GridActive
        {
            get { return _gridActive; }
            set
            {
                _gridActive = value;
            }
        }
        public string Time
        {
            get
            {
                string pattern = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
                string patternWithoutAmPm = pattern.Replace("tt", "");
                return DateTime.Now.ToString(patternWithoutAmPm);
            }
        }

        public bool ShowClock
        {
            get
            {
                return Settings.ShowClock;
            }
        }

        public double LargeFontSize
        {
            get { return Settings.FontScale * 60; }
        }
        public double MediumFontSize
        {
            get { return Settings.FontScale * 45; }
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
