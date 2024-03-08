using System;

[Serializable]
public class Settings
{
    //Graphic
    public bool disableHardwareAcceleration = false;
    public bool removeAnimations = false;
    public bool removeLoadingAnimation = false;
    public bool disableTransparency = false;
    public bool useCoverColor = false;

    //Loading
    public int loadingTimeWarning = 300;

    //SongPlayer
    public bool disableMediaKeys = false;
    public bool disableWindowsOverlay = false;
    public bool blockMaxVolume = false;
    public string audioDevice = "Default";

    //Misc
    public bool useDiscordRichPresence = false;
    public bool startHippocampWithWindows = true;
    public string hippocampTheme = "Default";
}