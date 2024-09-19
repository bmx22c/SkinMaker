using System.Diagnostics;
using WinProcess = System.Diagnostics.Process;

namespace SkinMaker;

internal static class Process
{
    public static string Start(string fileName, string arguments){
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,  // Example command to get .NET Core version
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new WinProcess();
        process.StartInfo = startInfo;

        // Start the process
        process.Start();
        process.StandardInput.Flush();
        process.StandardInput.Close();

        var processOutput = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return processOutput;
    }
}