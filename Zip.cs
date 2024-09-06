using System.IO.Compression;
using System.Text.RegularExpressions;

class Zip
{
    private static Utils utils = new Utils();

    public Zip(){}

    public string ZipFiles(string filePath, string Skin_Directory, string Skin_Name){
        string folderWork = Path.Combine(Skin_Directory);
        string folderSkin = Path.Combine(Skin_Directory.Replace("Work\\", ""));
        var filesWork = Directory.GetFiles(folderWork);
        var filesSkin = Directory.GetFiles(folderSkin);

        // Yeah I'll just one line this.
        // It select all JSON, DDS and .XXX.Gbx BUT NOT .mesh.gbx
        string filterPattern = $@"^[^\.]+\.json|[^\.]+\.dds|(?!.*\.mesh\.gbx$)[^\.]+\.[^\.]+\.gbx$";

        var filesToZip = filesWork.Where(file => Regex.IsMatch(Path.GetFileName(file).ToLower(), filterPattern)).ToList();

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

            int index = filePath.IndexOf("Work");
            string trackmaniaDocumentRootPath = filePath.Substring(0, index - 1);
            string destinationFolder = Path.Combine(trackmaniaDocumentRootPath, "Skins", "Models", "CarSport");
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
                utils.ExitWithMessage("No CarSport folder found. Please create it.");
            }

            File.Move(zipFileName, Path.Combine(destinationFolder, Skin_Name + ".zip"), true);
            return "Created zip file: " + Path.Combine(destinationFolder, Skin_Name + ".zip");
        }
        else
        {
            return "No files found in " + folderWork;
        }
    }
}