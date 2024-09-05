# SkinMaker
Automates the process of generating skin files by calling the Nadeo Importer by itself, calling [skinfix.exe](https://github.com/drunub/tm2020-skin-tools/releases/latest/) by itself, ZIP the generated files and move the ZIP file into the CarSport folder. Automatically.

## How to use
First, modify the `SkinMaker.dll.config` and specify where your Trackmania installation folder is.
To know that, you can open Trackmania, then press `CTRL+SHIFT+ESC`, under the "Processes" tab, right click on Trackmania and select "Open file location" (something along those lines, my Windows is in French). That's your TM installation folder.  
Copy it and paste it in the `TM_Install_Path` key, like so:  
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="TM_Install_Path" value="D:\Games\Epic Games\TrackmaniaNext" />
    <add key="LastExeModifiedDate" value="" />
    <add key="AutoCloseOnFinish" value="false" />
  </appSettings>
</configuration>
```

Leave the `LastExeModifiedDate` empty.

### Preparing your model

#### Template

(template link here)

#### Materials
All of your materials should be named according to one of the following patterns:
|        Material Name | Description |
|---------------------:|-------------|
|          SkinDmg_xxx |             |
|     SkinDmgDecal_xxx |             |
| DetailsDmgNormal_xxx | supports _N |
|  DetailsDmgDecal_xxx |             |
|    GlassDmgCrack_xxx |             |
|    GlassDmgDecal_xxx |             |
|             Gems_xxx |             |
|     GlassRefract_xxx |             |

Each material will use texture files `xxx_B.dds`, `xxx_D.dds` etc.

For example, a material called `SkinDmg_Skin` will use textures `Skin_D.dds`, `Skin_R.dds`.

### Export
Export your .blend file as .fbx. Make sure `Armature > Add Leaf Bones` is **Disabled**.

Prepare your .fbx within the `Documents/Trackmania/Work/` folder. Place your textures alongside it.  
The FBX file can be wherever your want, as long as it's inside the `Work` folder.

Then drag and drop your .fbx file onto the SkinMaker.exe and follow the instructions.

Showcase video:

https://github.com/bmx22c/SkinMaker/assets/6803869/60729405-bda0-4f37-be0d-7dcaff1c5e6f



## What it does
In order:
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
- [Nadeo Importer](https://doc.trackmania.com/create/nadeo-importer/01-download-and-install/) installed in your Trackmania installation folder
