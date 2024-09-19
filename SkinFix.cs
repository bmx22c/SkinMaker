using System.Configuration;
using System.Globalization;
using System.Text.Json;

namespace SkinMaker;

internal class SkinFix
{
    private static GitHubRoot? _skinFixInfo;

    public async Task DownloadSkinFix(string path, string? downloadUrl)
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

    public Task CheckDateSkinFix(string currentFolder)
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
                    Console.WriteLine("New skinfix.exe version found, downloading...");
                    DownloadSkinFix(Path.Combine(currentFolder, "skinfix.exe"), asset.browser_download_url).GetAwaiter()
                        .GetResult();
                    Console.WriteLine("done.");

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
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("skinfix.exe is up to date.");
                    Console.ResetColor();
                }
            }

            if (!foundSkinfix)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("skinfix.exe wasn't found in the latest release.");
                Console.ResetColor();
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
}