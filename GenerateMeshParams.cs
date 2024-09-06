using System.Xml;
using Assimp;

class GenerateMeshParams
{
    public GenerateMeshParams(string filePath, string Skin_Directory, string Skin_Name)
    {
        using (var context = new AssimpContext())
        {
            var scene = context.ImportFile(filePath);
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
                else if(currMat.StartsWith("GlassRefract_"))
                    material.SetAttribute("Model", "GlassRefract");
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
            doc.Save(Path.Combine(Skin_Directory, Skin_Name + ".MeshParams.xml"));
        }
    }
}