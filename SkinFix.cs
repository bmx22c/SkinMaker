using System.Configuration;
using System.Text.Json;

class SkinFix
{
    private static GitHubRoot skinFixInfo = null;
    private static string gitHubBrowserDownloadUrl = null;
    private static Utils utils = new Utils();
    public async Task DownloadSkinFix(string path)
    {
        if(gitHubBrowserDownloadUrl == null){
            utils.ExitWithMessage("Couldn't retrieve skinfix.exe download URL. Please download it manually at: https://github.com/drunub/tm2020-skin-tools/releases/latest/");
        }else{
            using (HttpClient client = new HttpClient())
            {
                // Download the file as a Stream
                using (Stream fileStream = await client.GetStreamAsync(gitHubBrowserDownloadUrl))
                {
                    // Copy the contents of the Stream to a file
                    using (FileStream outputFileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await fileStream.CopyToAsync(outputFileStream);
                    }
                }
            }
        }
    }
    public async Task CheckDateSkinFix(string currentFolder)
    {
        if(skinFixInfo == null){
            utils.ExitWithMessage("Couldn't retrieve skinfix.exe build date. Please download it manually at: https://github.com/drunub/tm2020-skin-tools/releases/latest/");
        }else{
            string LastExeModifiedDate = ConfigurationManager.AppSettings["LastExeModifiedDate"] ?? "";

            bool foundSkinfix = false;
            for (int i = 0; i < skinFixInfo.assets.Count; i++)
            {
                if(skinFixInfo.assets[i].name == "skinfix.exe"){
                    foundSkinfix = true;
                    gitHubBrowserDownloadUrl = skinFixInfo.assets[i].browser_download_url;

                    if(skinFixInfo.assets[i].updated_at.ToString() != LastExeModifiedDate){
                        Console.WriteLine("New skinfix.exe version found, downloading...");
                        DownloadSkinFix(Path.Combine(currentFolder, "skinfix.exe")).GetAwaiter().GetResult();
                        Console.WriteLine("New skinfix.exe version downloaded.");

                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");
                        if (appSettings != null)
                        {
                            // Modify the existing setting or add a new one
                            if (appSettings.Settings["LastExeModifiedDate"] != null)
                            {
                                appSettings.Settings["LastExeModifiedDate"].Value = skinFixInfo.assets[i].updated_at.ToString();
                            }
                            else
                            {
                                appSettings.Settings.Add("LastExeModifiedDate", skinFixInfo.assets[i].updated_at.ToString());
                            }

                            // Save the configuration file
                            config.Save(ConfigurationSaveMode.Modified);

                            // Refresh the appSettings section to reflect changes
                            ConfigurationManager.RefreshSection("appSettings");
                        }
                    }else{
                        Console.WriteLine("skinfix.exe is up to date.");
                    }
                }
            }

            if(!foundSkinfix){
                Console.WriteLine("skinfix.exe wasn't found in the latest release.");
            }
        }
    }

    public async Task GetSkinFixInfo()
    {
        using (HttpClient client = new HttpClient())
        {
            string url = "https://api.github.com/repos/drunub/tm2020-skin-tools/releases/latest";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:129.0) Gecko/20100101 Firefox/129.0");
            string json = await client.GetStringAsync(url);
            skinFixInfo = JsonSerializer.Deserialize<GitHubRoot>(json);
        }
    }

}