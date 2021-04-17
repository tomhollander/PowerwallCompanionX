﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace PowerwallCompanionX.Media
{
    public class SoundPlayer
    {
        public const string BatteryEmpty = "battery-empty.wav";
        public const string BatteryFull = "battery-full.wav";

        public void PlaySound(string file)
        {
            var stream = GetStreamFromFile(file);
            var audio = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            audio.Load(stream);
            audio.Play();
        }

        private Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("PowerwallCompanionX.Media." + filename);
            return stream;
        }
    }
}
