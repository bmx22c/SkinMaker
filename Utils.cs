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
        using var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        var key = key64.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (key == null) return null;
        List<string> installPath = new(); 
        
        foreach (var subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName))) {
            var displayName = subkey?.GetValue("DisplayName") as string;
            if (string.IsNullOrEmpty(displayName) || !displayName.Contains(findByName)) continue;
            installPath.Add(subkey?.GetValue("InstallLocation")?.ToString());
        }
        key.Close();
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