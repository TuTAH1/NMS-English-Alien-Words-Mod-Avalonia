# NMS-English-Alien-Words-Mod-Avalonia
The program for automatically making English Alien Words mod. 

# How to use

- Download [lastest release](https://github.com/TuTAH1/NMS-English-Alien-Words-Mod-Avalonia/releases/latest)
- Unpack archive to any folder
- Run `NMS-English-Alien-Words-Mod-Avalonia.exe`
- Choose lastest (or specific for your game version) mbin compiler version. Click `Download` button.
- Set your game's path
- Click `Create button`
- Wait until it's done
- Done. The mod is already created in `Mod` folder, you can play the game.

# How to update the mod
- In case if there's some drastic changes regarding alien localization happened, you can edit mod creation parametrs in the settings (as a user). I added as much user-controlled parametrs as possible.

## Third-party tools used

- [HGPAK tool](https://github.com/monkeyman192/HGPAKtool) (exe version) - for packing/unpacking .pak files
- [MBIN Compiler](https://github.com/monkeyman192/MBINCompiler) - for compiling/decompiling .mbin files

Mod wouldn't be possible without these tools.

## Requirements
- Windows x64
- .Net 8.0

## NuGet packages
- Avalonia `11.3.3`
- Avalonia.Desktop `11.3.3`
- Avalonia.Themes.Fluent `11.3.3`
- Avalonia.Fonts.Inter `11.3.3`
- Avalonia.Diagnostics `11.3.3` *(doesn't encluded in release build)*
- AvaloniaDialogs `3.6.1`
- bodong.Avalonia.PropertyGrid `11.3.3.2` *(Special thanks for this usefull lib that make creating settings interface almost automatic)*
- CommunityToolkit.Mvvm `8.4.0`
- FluentAvaloniaUI `2.2.0`
- LoadingIndicators.Avalonia `11.0.11.1`
- Markdown.Avalonia `11.0.2`
- Newtonsoft.Json `13.0.4`
- Octokit `14.0.0`
- SharpZipLib `1.4.2`


## Limitations / Known Issues

- HGPAK tool needs to be updated manually (by replacing .exe file). You can get one in No Mans Sky Modding Discord server.
- MBINCompiler may lock access on some files or ask for rewrite (despite `--quite` flag and no console window), but it only happens on repeated mod-creation without deleting the temp files
