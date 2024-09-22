using System.IO.Compression;
using System.Text.RegularExpressions;

namespace SkinMaker;

internal static class Zip
{
    public static string ZipFiles(string filePath, string skinDirectory, string skinName){
        var folderWork = Path.Combine(skinDirectory);
        var folderSkin = Path.Combine(skinDirectory.Replace("Work\\", ""));
        var filesWork = Directory.GetFiles(folderWork);
        var filesSkin = Directory.GetFiles(folderSkin);

        // select all JSON, DDS and .XXX.Gbx BUT NOT .mesh.gbx
        const string filterPattern = @"^[^\.]+\.json|[^\.]+\.dds|(?!.*\.mesh\.gbx$)[^\.]+\.[^\.]+\.gbx$";

        var filesToZip = filesWork.Where(file => Regex.IsMatch(Path.GetFileName(file).ToLower(), filterPattern)).ToList();

        // Add MainBody.Mesh.gbx to list of files
        filesToZip.AddRange(filesSkin.Where(file => !string.Equals(Path.GetFileName(file), (skinName + ".Mesh.Gbx"), StringComparison.CurrentCultureIgnoreCase)).ToList());

        if (filesToZip.Any())
        {
            var zipFileName = Path.Combine(folderWork, skinName + ".zip");

            using (var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var file in filesToZip)
                {
                    archive.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }

            var index = filePath.IndexOf("Work", StringComparison.Ordinal);
            var trackmaniaDocumentRootPath = filePath.Substring(0, index - 1);
            var destinationFolder = Path.Combine(trackmaniaDocumentRootPath, "Skins", "Models", "CarSport");
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
                Utils.ExitWithMessage("No CarSport folder found. Please create it.");
            }

            File.Move(zipFileName, Path.Combine(destinationFolder, skinName + ".zip"), true);
            return Path.Combine(destinationFolder, skinName + ".zip");
        }
        return string.Empty;
    }
}