using System.Configuration;

class Program
{
    private static GitHubRoot skinFixInfo = null;
    private static string gitHubBrowserDownloadUrl = null;
    private static Utils utils = new Utils();

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            utils.ExitWithMessage("Please provide a skin name.");
        }

        string SkinFbxPath = args[0];
        if(!File.Exists(SkinFbxPath)){
            utils.ExitWithMessage($"File {Path.GetFileName(SkinFbxPath)} doesn't exists.");
        }
        if(!SkinFbxPath.Contains("\\Work\\")){
            utils.ExitWithMessage("File isn't in a Work folder.");
        }
        string Skin_Name = Path.GetFileNameWithoutExtension(SkinFbxPath);
        string Skin_Name_Ext = Path.GetFileName(SkinFbxPath);
        string Skin_Directory = Path.GetDirectoryName(SkinFbxPath) ?? "";
        if(String.IsNullOrEmpty(Skin_Directory)){
            utils.ExitWithMessage("Unhandled error.");
        }
        
        string TM_Install_Path = ConfigurationManager.AppSettings["TM_Install_Path"] ?? "";

        if(String.IsNullOrEmpty(TM_Install_Path)){
            utils.ExitWithMessage("Please specify a TM_Install_Path variable in the SkinMaker.dll.config");
        }

        Console.WriteLine("\nGenerating " + Skin_Name + ".MesParams.xml based on " + Skin_Name_Ext + "...");
        new GenerateMeshParams(SkinFbxPath, Skin_Directory, Skin_Name);
        Console.WriteLine(Skin_Name + ".MeshParams.xml generation OK.");

        SkinFix sf = new SkinFix();
        sf.GetSkinFixInfo().GetAwaiter().GetResult();
        string currentFolder = AppDomain.CurrentDomain.BaseDirectory;
        if(!File.Exists(Path.Combine(currentFolder, "skinfix.exe"))){
            Console.WriteLine("\nskinfix.exe not found. Attempting to download...");
            sf.DownloadSkinFix(Path.Combine(currentFolder, "skinfix.exe")).GetAwaiter().GetResult();
            Console.WriteLine("skinfix.exe downloaded. Continuing.");
        }else{
            Console.WriteLine("\nChecking skinfix.exe last modified date...");
            sf.CheckDateSkinFix(currentFolder).GetAwaiter().GetResult();
        }
        string Converter_Exe_Path = Path.Combine(currentFolder, "skinfix.exe");

        if (!File.Exists(Path.Combine(Skin_Directory, Skin_Name + ".MeshParams.xml")))
        {
            utils.ExitWithMessage(Skin_Name + ".MeshParams.xml doesn't exist.");
        }

        Console.WriteLine("\nStarting NadeoImporter process...");

        int index = SkinFbxPath.IndexOf("Work") + "Work".Length;
        string skinRelativePath = SkinFbxPath.Substring(index);
        int lastSlashIndex = skinRelativePath.LastIndexOf("\\");
        skinRelativePath = skinRelativePath.Substring(0, lastSlashIndex);

        string nadeoImporterOutput = new Process().Start(Path.Combine(TM_Install_Path, "NadeoImporter.exe"), "Mesh " + Path.Combine(skinRelativePath, Skin_Name_Ext));
        if(!nadeoImporterOutput.Split('\n').Reverse().Skip(1).First().StartsWith("Created :user:") && !nadeoImporterOutput.Split('\n').Reverse().Skip(1).First().EndsWith(".Mesh.gbx")){
            Console.WriteLine(nadeoImporterOutput);
            utils.ExitWithMessage("NadeoImporter failed, check the output above.");
        }else{
            Console.WriteLine("NadeoImporter process OK.");
        }


        Console.WriteLine("\nStarting skinfix process...");
        new Process().Start(Converter_Exe_Path, Path.Combine(Path.GetDirectoryName(SkinFbxPath.Replace("Work\\", "")), Skin_Name + ".Mesh.gbx") + " --out " + Path.Combine(Path.GetDirectoryName(SkinFbxPath.Replace("Work\\", "")), "MainBody.Mesh.Gbx"));
        Console.WriteLine("skinfix process OK...");

        Console.WriteLine("\nZipping files...");
        Console.WriteLine(new Zip().ZipFiles(SkinFbxPath, Skin_Directory, Skin_Name));

        Console.WriteLine("\nSkin created successfully!");
        bool AutoCloseOnFinish = bool.Parse(ConfigurationManager.AppSettings["AutoCloseOnFinish"] ?? "false");
        if(!AutoCloseOnFinish){
            Console.Write("Press any key to close..."); Console.ReadLine();
        }
    }
}