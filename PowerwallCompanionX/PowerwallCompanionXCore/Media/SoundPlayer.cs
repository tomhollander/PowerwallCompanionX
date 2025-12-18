using Plugin.Maui.Audio;

namespace PowerwallCompanionX.Media
{
    public class SoundPlayer
    {
        public const string BatteryEmpty = "Assets/battery-empty.wav";
        public const string BatteryFull = "Assets/battery-full.wav";

        public async void PlaySound(string file)
        {
            // MauiAsset files must be read as a stream
            var stream = await FileSystem.OpenAppPackageFileAsync(file);
            var audioPlayer = AudioManager.Current.CreatePlayer(stream);
            audioPlayer.Play();
        }

    }
}
