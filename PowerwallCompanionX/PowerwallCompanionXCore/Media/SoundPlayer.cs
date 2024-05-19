using Plugin.Maui.Audio;

namespace PowerwallCompanionX.Media
{
    public class SoundPlayer
    {
        public const string BatteryEmpty = "battery-empty.wav";
        public const string BatteryFull = "battery-full.wav";

        public void PlaySound(string file)
        {
            var audioPlayer = AudioManager.Current.CreatePlayer(file);
            audioPlayer.Play();
        }

    }
}
