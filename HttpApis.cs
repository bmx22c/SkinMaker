using System.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace SkinMaker;

internal class HttpApis
{
    private static GitHubRoot? _skinFixInfo;

    public static async Task DownloadSkinFix(string path, string? downloadUrl)
    {
        if (downloadUrl == null)
        {
            if (_skinFixInfo?.assets != null)
                foreach (var asset in _skinFixInfo.assets)
                {
                    if (asset.name == "skinfix.exe")
                    {
                        downloadUrl = asset.browser_download_url;
                        break;
                    }
                }
        }

        if (downloadUrl == null)
        {
            Utils.ExitWithMessage(
                "Couldn't retrieve skinfix.exe download URL. " +
                "\nPlease download it manually at: https://github.com/drunub/tm2020-skin-tools/releases/latest/");
        }

        using HttpClient client = new HttpClient();
        // Download the file as a Stream
        await using var fileStream = await client.GetStreamAsync(downloadUrl);
        // Copy the contents of the Stream to a file
        await using var outputFileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(outputFileStream);
    }

    public static Task CheckDateSkinFix(string currentFolder)
    {
        if (_skinFixInfo == null)
        {
            Utils.ExitWithMessage(
                "Couldn't retrieve skinfix.exe build date. " +
                "\nPlease download it manually at: https://github.com/drunub/tm2020-skin-tools/releases/latest/");
        }
        else
        {
            var lastExeModifiedDate = ConfigurationManager.AppSettings["LastExeModifiedDate"] ?? "";
            var foundSkinfix = false;

            foreach (var asset in _skinFixInfo.assets)
            {
                if (asset.name != "skinfix.exe") continue;
                foundSkinfix = true;

                if (asset.updated_at.ToString(CultureInfo.InvariantCulture) != lastExeModifiedDate)
                {
                    Utils.WriteLine("New skinfix.exe version found, downloading...");
                    DownloadSkinFix(Path.Combine(currentFolder, "skinfix.exe"), asset.browser_download_url).GetAwaiter()
                        .GetResult();
                    Utils.WriteLine("Done.", ConsoleColor.Green);

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var appSettings = (AppSettingsSection)config.GetSection("appSettings");
                    if (appSettings == null) continue;
                    // Modify the existing setting or add a new one
                    if (appSettings.Settings["LastExeModifiedDate"] != null)
                    {
                        appSettings.Settings["LastExeModifiedDate"].Value =
                            asset.updated_at.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        appSettings.Settings.Add("LastExeModifiedDate",
                            asset.updated_at.ToString(CultureInfo.InvariantCulture));
                    }

                    // Save the configuration file
                    config.Save(ConfigurationSaveMode.Modified);

                    // Refresh the appSettings section to reflect changes
                    ConfigurationManager.RefreshSection("appSettings");
                }
                else
                {
                    Utils.WriteLine("skinfix.exe is up to date.", ConsoleColor.Green);
                }
            }

            if (!foundSkinfix)
            {
                Utils.WriteLine("skinfix.exe wasn't found in the latest release.", ConsoleColor.Green);
            }
        }

        return Task.CompletedTask;
    }

    public static async Task GetSkinFixInfo()
    {
        using var client = new HttpClient();
        const string url = "https://api.github.com/repos/drunub/tm2020-skin-tools/releases/latest";
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:129.0) Gecko/20100101 Firefox/129.0");
        var json = await client.GetStringAsync(url);
        _skinFixInfo = JsonSerializer.Deserialize<GitHubRoot>(json);
    }

    public static async Task GetNadeoImporter(string tmPath)
    {
        const string file = "NadeoImporter_2022_07_12.zip";
        const string url = "https://nadeo-download.cdn.ubi.com/trackmania/" + file;

        try
        {
            if (!File.Exists(Path.Combine(tmPath, file)))
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:129.0) Gecko/20100101 Firefox/129.0");
                Utils.WriteLine($"Downloading {file} to {tmPath}");

                var blob = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(Path.Combine(tmPath, file), blob);
                Utils.WriteLine("Done.", ConsoleColor.Green);
            }

            Utils.WriteLine("Extracting...");
            using (var archive = ZipFile.OpenRead(Path.Combine(tmPath, file)))
            {
                foreach (var entry in archive.Entries)
                {
                    var destinationPath = Path.GetFullPath(Path.Combine(tmPath, entry.Name));
                    if (string.IsNullOrEmpty(entry.Name)) continue;
                    Utils.WriteLine(destinationPath + "...", ConsoleColor.DarkCyan);
                    entry.ExtractToFile(destinationPath, true);
                }
            }

            Utils.WriteLine("Done.", ConsoleColor.Green);
        }
        catch (Exception e)
        {
            Utils.WriteLine("Please check the errors and resolve unzip manually:");
            Utils.ExitWithMessage(e.Message);
            
        }
    }
}