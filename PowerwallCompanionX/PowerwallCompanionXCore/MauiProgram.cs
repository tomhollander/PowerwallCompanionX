

using Plugin.Maui.Audio;

namespace PowerwallCompanionX;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();
        builder.Services.AddSingleton(AudioManager.Current);

        return builder.Build();
    }
}
