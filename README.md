# SkinMaker
Automates the process of generating skin files by calling the Nadeo Importer by itself, calling [skinfix.exe](https://github.com/drunub/tm2020-skin-tools/releases/latest/) by itself, ZIP the generated files and move the ZIP file into the CarSport folder. Automatically.

## How to use
On first launch, the program will try and prompt to know where your Trackmania is installed. If it cannot find it, you'll need to modify the `SkinMaker.dll.config`.
To know that, you can open Trackmania, then press `CTRL+SHIFT+ESC`, under the "Processes" tab, right click on Trackmania and select "Open file location" (something along those lines, my Windows is in French). That's your TM installation folder.  
Copy it and paste it in the `TM_Install_Path` key, like so:  
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="TM_Install_Path" value="D:\Games\Epic Games\TrackmaniaNext" />
    <add key="LastExeModifiedDate" value="" />
    [...]
  </appSettings>
</configuration>
```

Leave the `LastExeModifiedDate` empty.

If you don't have Nadeo Importer installed, the program will prompt your to install it.

Prepare your .fbx within the `Documents/Trackmania/Work/` folder. Place your textures alongside it.  
The FBX file can be wherever your want, as long as it's inside the `Work` folder.

Then drag and drop your .fbx file onto the SkinMaker.exe and follow the instructions.

Showcase video:

https://github.com/bmx22c/SkinMaker/assets/6803869/60729405-bda0-4f37-be0d-7dcaff1c5e6f

## Options
- `TM_Install_Path`: Set to nothing to force the Trackmania installation path prompt,
- `LastExeModifiedDate`: Set to nothing to force download the `skinfix.exe`,
- `AutoCloseOnFinish`: Set to `true` to auto close the program on successful completion,
- `AddTimestampToZip`: Set to true to add a timetamp suffix on the ZIP file name,
- `AskOpenFileLocation`: Set to false to disable the prompt to open the path to the ZIP on successful completion.

## What it does
In order:
- Check if `TM_Install_Path` is set, otherwise it'll try to guess where it's installed,
- Check if Nadeo Importer is installed, otherwise it'll prompt to try and download it
- Checks if `skinfix.exe` is downloaded
    - If yes, check if it's up to date. If it's not, it'll download the latest version
    - If no, it'll download the latest version
- Opens the fbx file you passed
- Generates the .MeshParams.xml dynamically based on the materials inside your fbx
- Call NadeoImporter to generate the first part of the skin
- Call `skinfix.exe` to convert the file to TM2020 skin
- ZIP all necessary files
- Move ZIP into CarSport folder

## Build
Modify the code.
Build with `dotnet build`.  
Run it with `.\bin\Debug\net6.0\SkinMaker.exe "{path to fbx file inside a Documents/Trackmania/Work/ folder}"`.  
Create release with `dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true`

## Requirements
- [Nadeo Importer](https://doc.trackmania.com/create/nadeo-importer/01-download-and-install/) installed in your Trackmania installation folder (the program will try and download it for you)
