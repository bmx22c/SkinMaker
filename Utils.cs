using System.Xml;
using Assimp;
using Microsoft.Win32;

namespace SkinMaker;

internal static class Utils
{
    public static void ExitWithMessage(string message){
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.Write("Press any key to close..."); Console.ReadKey();
        Environment.Exit(0);
    }
    
    public static List<string> CheckInstalled(string findByName){
        List<string> installPath = new(); 

        string[] registryKeys = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (string registryKey in registryKeys)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (key == null) continue;

                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        if (subKey != null)
                        {
                            string displayName = (string)subKey.GetValue("DisplayName");
                            string installLocation = (string)subKey.GetValue("InstallLocation");

                            if (!string.IsNullOrEmpty(displayName) && displayName.Contains(findByName) && !string.IsNullOrEmpty(installLocation))
                                installPath.Add($"{displayName} - {installLocation}");

                        }
                    }
                }
            }
        }
        return installPath;
    }

    public static void GenerateMeshParams(string filePath, string skinDirectory, string skinName)
    {
        using var context = new AssimpContext();
        var scene = context.ImportFile(filePath);
        var fbxMaterials = scene.Materials;
        var doc = new XmlDocument();

        // Create the root element and add it to the document
        var root = doc.CreateElement("MeshParams");
        doc.AppendChild(root);

        // Set the attributes of the root element
        root.SetAttribute("MeshType", "Vehicle");
        root.SetAttribute("SkelSocketPrefix", "_");

        // Create the Materials element and add it to the root element
        var materials = doc.CreateElement("Materials");
        root.AppendChild(materials);
            
        foreach (var fbxMat in fbxMaterials)
        {
            var xmlMaterial = doc.CreateElement("Material");
            var currMat = fbxMat.Name;
            xmlMaterial.SetAttribute("Name", currMat);

            if(currMat.StartsWith("SkinDmg_"))
                xmlMaterial.SetAttribute("Model", "SkinDmg");
            else if(currMat.StartsWith("DetailsDmgNormal_"))
                xmlMaterial.SetAttribute("Model", "DetailsDmgNormal");
            else if(currMat.StartsWith("GlassDmgCrack_"))
                xmlMaterial.SetAttribute("Model", "GlassDmgCrack");
            else if(currMat.StartsWith("GlassDmgDecal_"))
                xmlMaterial.SetAttribute("Model", "GlassDmgDecal");
            else if(currMat.StartsWith("DetailsDmgDecal_"))
                xmlMaterial.SetAttribute("Model", "DetailsDmgDecal");
            else if(currMat.StartsWith("SkinDmgDecal_"))
                xmlMaterial.SetAttribute("Model", "SkinDmgDecal");
            else if(currMat.StartsWith("Gems_"))
                xmlMaterial.SetAttribute("Model", "Gems");
            else if(currMat.StartsWith("GlassRefract_"))
                xmlMaterial.SetAttribute("Model", "GlassRefract");
            else
                xmlMaterial.SetAttribute("Model", "DetailsDmgNormal");
                
            materials.AppendChild(xmlMaterial);
        }

        // Create the Constants, UvAnims, VisibleIds, and Color elements and add them to the root element
        root.AppendChild(doc.CreateElement("Constants"));
        root.AppendChild(doc.CreateElement("UvAnims"));
        root.AppendChild(doc.CreateElement("VisibleIds"));
        root.AppendChild(doc.CreateElement("Color"));

        // Save the XML document to a file
        doc.Save(Path.Combine(skinDirectory, skinName + ".MeshParams.xml"));
    }
      
}