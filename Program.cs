using System.Configuration;

namespace SkinMaker;

class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("SkinMaker version: 1.3.0");
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: SkinMaker.exe <path-to-skin-file>.fbx");
            return;
        }
        
        var skinFbxPath = args[0];
        if (!File.Exists(skinFbxPath))
        {
            Utils.ExitWithMessage($"File '{Path.GetFileName(skinFbxPath)}' not exists.");
        }

        if (!skinFbxPath.Contains(@"\Work\"))
        {
            Utils.ExitWithMessage("File isn't in a Work folder.");
        }

        var skinName = Path.GetFileNameWithoutExtension(skinFbxPath);
        var skinNameExt = Path.GetFileName(skinFbxPath);
        var skinDirectory = Path.GetDirectoryName(skinFbxPath) ?? "";
        if (string.IsNullOrEmpty(skinDirectory))
        {
            Utils.ExitWithMessage("Unhandled error.");
        }

        var tmInstallPath = ConfigurationManager.AppSettings["TM_Install_Path"];
        if (string.IsNullOrEmpty(tmInstallPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("TM_Install_Path variable in the SkinMaker.dll.config is empty!");
            Console.ResetColor();
            Console.WriteLine("Trying to autodetect: ");
            var i = 0;
            var tmPath = Utils.CheckInstalled("Trackmania");
            foreach (var arg in tmPath)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine((i + 1) + ": ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(arg+"\n");
                Console.ResetColor();
                i += 1;
            }
            Console.ResetColor();
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
                Console.WriteLine("TM_Install_Path setting updated: " + tmInstallPath);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        if (tmInstallPath == null || !File.Exists(Path.Combine(tmInstallPath, "NadeoImporter.exe")))
        {
            Utils.ExitWithMessage("Can't find NadeoImporter.exe at Trackmania path. Please install latest from:" +
                                  "\nhttps://nadeo-download.cdn.ubi.com/trackmania/NadeoImporter_2022_07_12.zip");
        }

        Console.WriteLine("\nGenerating " + skinName + ".MesParams.xml based on " + skinNameExt + "...");
        Utils.GenerateMeshParams(skinFbxPath, skinDirectory, skinName);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(skinName + ".MeshParams.xml generation OK.");
        Console.ResetColor();
        
        var sf = new SkinFix();
        SkinFix.GetSkinFixInfo().GetAwaiter().GetResult();
        var currentFolder = AppDomain.CurrentDomain.BaseDirectory;
        if (!File.Exists(Path.Combine(currentFolder, "skinfix.exe")))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nskinfix.exe not found. Attempting to download...");
            Console.ResetColor();
            sf.DownloadSkinFix(Path.Combine(currentFolder, "skinfix.exe"), null).GetAwaiter().GetResult();
            Console.WriteLine("skinfix.exe downloaded. Continuing.");
        }
        else
        {
            Console.WriteLine("\nChecking skinfix.exe last modified date...");
            sf.CheckDateSkinFix(currentFolder).GetAwaiter().GetResult();
        }

        var converterExePath = Path.Combine(currentFolder, "skinfix.exe");

        if (!File.Exists(Path.Combine(skinDirectory, skinName + ".MeshParams.xml")))
        {
            Utils.ExitWithMessage(skinName + ".MeshParams.xml doesn't exist.");
        }

        Console.WriteLine("\nStarting NadeoImporter process...");

        var index = skinFbxPath.IndexOf("Work", StringComparison.Ordinal) + "Work".Length;
        var skinRelativePath = skinFbxPath.Substring(index);
        var lastSlashIndex = skinRelativePath.LastIndexOf("\\", StringComparison.Ordinal);
        skinRelativePath = skinRelativePath.Substring(0, lastSlashIndex);

        if (tmInstallPath == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nTM Install Path is for some reason still empty, can't continue.");
            Console.ResetColor();
            return;
        }

        var nadeoImporterOutput = Process.Start(Path.Combine(tmInstallPath, "NadeoImporter.exe"),
            "Mesh " + Path.Combine(skinRelativePath, skinNameExt));
        if (!nadeoImporterOutput.Split('\n').Reverse().Skip(1).First().StartsWith("Created :user:") &&
            !nadeoImporterOutput.Split('\n').Reverse().Skip(1).First().EndsWith(".Mesh.gbx"))
        {
            Console.WriteLine(nadeoImporterOutput);
            Utils.ExitWithMessage("NadeoImporter failed, check the output above.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("NadeoImporter process OK.");
            Console.ResetColor();
        }


        Console.WriteLine("\nStarting skinfix process...");
        Process.Start(converterExePath,
            Path.Combine(Path.GetDirectoryName(skinFbxPath.Replace("Work\\", "")) ?? string.Empty,
                skinName + ".Mesh.gbx") + " --out " +
            Path.Combine(Path.GetDirectoryName(skinFbxPath.Replace("Work\\", "")) ?? string.Empty,
                "MainBody.Mesh.Gbx"));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("skinfix process OK...");
        Console.ResetColor();
        
        Console.WriteLine("\nZipping files...");
        Console.WriteLine(Zip.ZipFiles(skinFbxPath, skinDirectory, skinName));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nSkin created successfully!");
        Console.ResetColor();
        var autoCloseOnFinish = bool.Parse(ConfigurationManager.AppSettings["AutoCloseOnFinish"] ?? "false");
        if (autoCloseOnFinish) return;
        Console.Write("Press any key to close...");
        Console.ReadKey();
    }
}