﻿using System.Configuration;

namespace SkinMaker;

internal static class Program
{
    private static void Main(string[] args)
    {
        Utils.WriteLine("SkinMaker version: 1.3.0");
        if (args.Length == 0)
        {
            Utils.WriteLine("Usage: SkinMaker.exe <path-to-skin-file>.fbx");
            return;
        }

        var skinFbxPath = args[0];
        if (!File.Exists(skinFbxPath))
            Utils.ExitWithMessage($"File '{Path.GetFileName(skinFbxPath)}' not exists.");

        if (!skinFbxPath.Contains(@"\Work\"))
            Utils.ExitWithMessage("File isn't in a Work folder.");

        var skinName = Path.GetFileNameWithoutExtension(skinFbxPath);
        var skinNameExt = Path.GetFileName(skinFbxPath);
        var skinDirectory = Path.GetDirectoryName(skinFbxPath) ?? "";
        if (string.IsNullOrEmpty(skinDirectory))
            Utils.ExitWithMessage("Unhandled error.");


        var tmInstallPath = ConfigurationManager.AppSettings["TM_Install_Path"];
        if (string.IsNullOrEmpty(tmInstallPath))
        {
            Utils.WriteLine("TM_Install_Path variable in the SkinMaker.dll.config is empty!", ConsoleColor.Red);
            Utils.WriteLine("Trying to autodetect: ");
            var i = 0;
            var tmPath = Utils.CheckInstalled("Trackmania");
            foreach (var arg in tmPath)
            {
                Utils.WriteLine((i + 1) + ": ", ConsoleColor.White);
                Utils.WriteLine(arg + "\n", ConsoleColor.Yellow);
                i += 1;
            }

            Console.Write("Please choose install location by number or press enter to exit: ");
            var answer = Console.ReadLine();
            var success = int.TryParse(answer, out var res);
            if (!success || answer == null || answer.ToLower() == "q")
                Utils.ExitWithMessage("Can't find Trackmania installation.");
            if (res > 0 && res <= tmPath.Count) tmInstallPath = tmPath[res - 1];
            else Utils.ExitWithMessage("Invalid Range.");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = (AppSettingsSection)config.GetSection("appSettings");
            if (appSettings != null)
            {
                appSettings.Settings["TM_Install_Path"].Value = tmInstallPath;
                config.Save(ConfigurationSaveMode.Modified);
                Utils.WriteLine("TM_Install_Path setting updated: " + tmInstallPath);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        if (tmInstallPath == null)
        {
            Utils.ExitWithMessage("Failed to find Trackmania installation.");
            return;
        }

        if (!File.Exists(Path.Combine(tmInstallPath, "NadeoImporter.exe")))
        {
            Utils.WriteLine("Unable to find NadeoImporter.exe, attempt to download and install (y/n) ?",
                ConsoleColor.Yellow);
            var answer = Console.ReadLine();
            if (answer?.ToLower() != "y")
                Utils.ExitWithMessage("Can't find NadeoImporter.exe at Trackmania path. Please install latest from:" +
                                      "\nhttps://nadeo-download.cdn.ubi.com/trackmania/NadeoImporter_2022_07_12.zip");

            HttpApis.GetNadeoImporter(tmInstallPath).GetAwaiter().GetResult();

            if (!File.Exists(Path.Combine(tmInstallPath, "NadeoImporter.exe")))
            {
                Utils.ExitWithMessage(
                    "After download we still can't find NadeoImporter.exe at Trackmania path. Please install latest from:" +
                    "\nhttps://nadeo-download.cdn.ubi.com/trackmania/NadeoImporter_2022_07_12.zip");
            }
        }

        Utils.WriteLine("\nGenerating " + skinName + ".MesParams.xml based on " + skinNameExt + "...");
        Utils.GenerateMeshParams(skinFbxPath, skinDirectory, skinName);
        Utils.WriteLine(skinName + ".MeshParams.xml generation OK.", ConsoleColor.Green);

        HttpApis.GetSkinFixInfo().GetAwaiter().GetResult();
        var currentFolder = AppDomain.CurrentDomain.BaseDirectory;
        if (!File.Exists(Path.Combine(currentFolder, "skinfix.exe")))
        {
            Utils.WriteLine("\nskinfix.exe not found. Attempting to download...", ConsoleColor.Yellow);
            HttpApis.DownloadSkinFix(Path.Combine(currentFolder, "skinfix.exe"), null).GetAwaiter().GetResult();
            Utils.WriteLine("skinfix.exe downloaded. Continuing.");
        }
        else
        {
            Utils.WriteLine("\nChecking skinfix.exe last modified date...");
            HttpApis.CheckDateSkinFix(currentFolder).GetAwaiter().GetResult();
        }

        var converterExePath = Path.Combine(currentFolder, "skinfix.exe");

        if (!File.Exists(Path.Combine(skinDirectory, skinName + ".MeshParams.xml")))
        {
            Utils.ExitWithMessage(skinName + ".MeshParams.xml doesn't exist.");
        }

        Utils.WriteLine("\nStarting NadeoImporter process...");

        var index = skinFbxPath.IndexOf("Work", StringComparison.Ordinal) + "Work".Length;
        var skinRelativePath = skinFbxPath.Substring(index);
        var lastSlashIndex = skinRelativePath.LastIndexOf("\\", StringComparison.Ordinal);
        skinRelativePath = skinRelativePath.Substring(0, lastSlashIndex);

        var nadeoImporterOutput = Process.Start(Path.Combine(tmInstallPath, "NadeoImporter.exe"),
            "Mesh " + Path.Combine(skinRelativePath, skinNameExt));
        if (!nadeoImporterOutput.Split('\n').Reverse().Skip(1).First().StartsWith("Created :user:") &&
            !nadeoImporterOutput.Split('\n').Reverse().Skip(1).First().EndsWith(".Mesh.gbx"))
        {
            Utils.WriteLine(nadeoImporterOutput);
            Utils.ExitWithMessage("NadeoImporter failed, check the output above.");
        }
        else
        {
            Utils.WriteLine("NadeoImporter process OK.", ConsoleColor.Green);
        }


        Utils.WriteLine("\nStarting skinfix process...");
        Process.Start(converterExePath,
            Path.Combine(Path.GetDirectoryName(skinFbxPath.Replace("Work\\", "")) ?? string.Empty,
                skinName + ".Mesh.gbx") + " --out " +
            Path.Combine(Path.GetDirectoryName(skinFbxPath.Replace("Work\\", "")) ?? string.Empty,
                "MainBody.Mesh.Gbx"));
        Utils.WriteLine("skinfix process OK...", ConsoleColor.Green);

        Utils.WriteLine("\nZipping files...");
        var path = Zip.ZipFiles(skinFbxPath, skinDirectory, skinName);
        Utils.WriteLine("\nSkin created successfully!", ConsoleColor.Green);
        Utils.WriteLine("Skin file location: " + path);
        var askOpenFileLocation = bool.Parse(ConfigurationManager.AppSettings["AskOpenFileLocation"] ?? "true");
        if(askOpenFileLocation)
        {
            Utils.WriteLine("\nOpen file location folder (y/n)?");
            if (Console.ReadLine() == "y") Process.Start("explorer.exe", "/select," + path);
        }
        var autoCloseOnFinish = bool.Parse(ConfigurationManager.AppSettings["AutoCloseOnFinish"] ?? "false");
        if (autoCloseOnFinish) return;
        Console.Write("Press any key to close...");
        Console.ReadKey();
    }
}