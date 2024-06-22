using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;
using System.Configuration;
using System.Net;
using Assimp;
using System.Xml;

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
        
        string TM_Install_Path = ConfigurationManager.AppSettings["TM_Install_Path"];
        string TM_User_Path = ConfigurationManager.AppSettings["TM_User_Path"];

        using (var context = new AssimpContext())
        {
            var scene = context.ImportFile(Path.Combine(TM_User_Path, "Work", "Skins", "Models", Skin_Name, Skin_Name + ".fbx"));
            var fbxMaterials = scene.Materials;
            XmlDocument doc = new XmlDocument();

            // Create the root element and add it to the document
            XmlElement root = doc.CreateElement("MeshParams");
            doc.AppendChild(root);

            // Set the attributes of the root element
            root.SetAttribute("MeshType", "Vehicle");
            root.SetAttribute("SkelSocketPrefix", "_");

            // Create the Materials element and add it to the root element
            XmlElement materials = doc.CreateElement("Materials");
            root.AppendChild(materials);
            
            for (int i = 0; i < fbxMaterials.Count; i++)
            {
                XmlElement material = doc.CreateElement("Material");
                string currMat = fbxMaterials[i].Name;
                material.SetAttribute("Name", currMat);

                if(currMat.StartsWith("SkinDmg_"))
                    material.SetAttribute("Model", "SkinDmg");
                else if(currMat.StartsWith("DetailsDmgNormal_"))
                    material.SetAttribute("Model", "DetailsDmgNormal");
                else if(currMat.StartsWith("GlassDmgCrack_"))
                    material.SetAttribute("Model", "GlassDmgCrack");
                else if(currMat.StartsWith("GlassDmgDecal_"))
                    material.SetAttribute("Model", "GlassDmgDecal");
                else if(currMat.StartsWith("DetailsDmgDecal_"))
                    material.SetAttribute("Model", "DetailsDmgDecal");
                else if(currMat.StartsWith("SkinDmgDecal_"))
                    material.SetAttribute("Model", "SkinDmgDecal");
                else if(currMat.StartsWith("Gems_"))
                    material.SetAttribute("Model", "Gems");
                else
                    material.SetAttribute("Model", "DetailsDmgNormal");
                
                materials.AppendChild(material);
            }

            // Create the Constants, UvAnims, VisibleIds, and Color elements and add them to the root element
            root.AppendChild(doc.CreateElement("Constants"));
            root.AppendChild(doc.CreateElement("UvAnims"));
            root.AppendChild(doc.CreateElement("VisibleIds"));
            root.AppendChild(doc.CreateElement("Color"));

            // Save the XML document to a file
            doc.Save(Path.Combine(TM_User_Path, "Work", "Skins", "Models", Skin_Name, Skin_Name + ".MeshParams.xml"));
        }

        string currentFolder = AppDomain.CurrentDomain.BaseDirectory;
        Console.WriteLine(currentFolder);
        if(!File.Exists(Path.Combine(currentFolder, "skinfix.exe"))){
            Console.WriteLine("skinfix.exe not found. Attempting to download...");
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(@"https://openplanet.dev/file/119/download", Path.Combine(currentFolder, "skinfix.exe"));
            }
            Console.WriteLine("skinfix.exe downloaded. Continuing.");
        }

        string Converter_Exe_Path = Path.Combine(currentFolder, "skinfix.exe");

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
        string folderWork = Path.Combine(TM_User_Path, "Work", "Skins", "Models", Skin_Name);
        string folderSkin = Path.Combine(TM_User_Path, "Skins", "Models", Skin_Name);
        var filesWork = Directory.GetFiles(folderWork);
        var filesSkin = Directory.GetFiles(folderSkin);
        var filesToZip = filesWork.Where(file =>
            !Path.GetFileName(file).ToLower().Contains("meshparams") &&
            !Path.GetFileName(file).ToLower().EndsWith(".fbx") &&
            Path.GetFileName(file).ToLower() != (Skin_Name + ".Mesh.Gbx").ToLower()
        ).ToList();

        // Add MainBody.Mesh.gbx to list of files
        filesToZip.AddRange(filesSkin.Where(file => Path.GetFileName(file).ToLower() != (Skin_Name + ".Mesh.Gbx").ToLower()).ToList());

        if (filesToZip.Any())
        {
            string zipFileName = Path.Combine(folderWork, Skin_Name + ".zip");

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
            Console.WriteLine("No files found in " + folderWork + " with extensions: " + string.Join(", ", fileExtensions));
        }
    }
}