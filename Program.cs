using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a skin name.");
            Environment.Exit(0);
        }

        string Skin_Name = args[0];
        bool fakeshad = args.Length > 1 && args[1].ToLower() == "--fakeshad";

        string TM_Install_Path = @"D:\Games\Epic Games\TrackmaniaNext";
        string TM_User_Path = @"C:\Users\bmx22\Documents\Trackmania2020";
        string Converter_Exe_Path = @"C:\Users\bmx22\Downloads\test exe\skin_script.exe";

        if (!File.Exists(Path.Combine(TM_User_Path, "Work", "Skins", "Models", Skin_Name, Skin_Name + ".MeshParams.xml")))
        {
            Console.WriteLine(Skin_Name + ".MeshParams.xml doesn't exist.");
            Environment.Exit(0);
        }

        Process.Start(Path.Combine(TM_Install_Path, "NadeoImporter.exe"), "Mesh " + "Skins\\Models\\" + Skin_Name + "\\" + Skin_Name + ".fbx").WaitForExit();

        if (fakeshad)
        {
            Process.Start(Converter_Exe_Path, Path.Combine(TM_User_Path, "Skins", "Models", Skin_Name, Skin_Name + ".Mesh.gbx") + " --fakeshad").WaitForExit();
        }
        else
        {
            Process.Start(Converter_Exe_Path, Path.Combine(TM_User_Path, "Skins", "Models", Skin_Name, Skin_Name + ".Mesh.gbx")).WaitForExit();
        }

        string[] fileExtensions = { "MainBody.Mesh.Gbx" };
        string folder = Path.Combine(TM_User_Path, "Skins", "Models", Skin_Name);
        var files = Directory.GetFiles(folder);
        for(int i = 0; i < files.Length; i++){
            Console.WriteLine(files[i]);
        }
        var filesToZip = files.Where(file => Path.GetFileName(file).ToLower() != (Skin_Name + ".Mesh.Gbx").ToLower());

        if (filesToZip.Any())
        {
            string zipFileName = Path.Combine(folder, Skin_Name + ".zip");

            using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var file in filesToZip)
                {
                    archive.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }

            string destinationFolder = Path.Combine(TM_User_Path, "Skins", "Models", "CarSport");
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
                Console.WriteLine("No CarSport folder found. Please create it.");
                Environment.Exit(0);
            }

            File.Move(zipFileName, Path.Combine(destinationFolder, Skin_Name + ".zip"), true);
            Console.WriteLine("Created zip file: " + Path.Combine(destinationFolder, Skin_Name + ".zip"));
        }
        else
        {
            Console.WriteLine("No files found in " + folder + " with extensions: " + string.Join(", ", fileExtensions));
        }
    }
}