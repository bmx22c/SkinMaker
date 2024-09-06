using System.Diagnostics;
using WinProcess = System.Diagnostics.Process;

class Process
{
    public Process(){}

    public string Start(string fileName, string arguments){
        string processOutput = "";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,  // Example command to get .NET Core version
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (WinProcess process = new WinProcess())
        {
            process.StartInfo = startInfo;

            // Start the process
            process.Start();
            process.StandardInput.Flush();
            process.StandardInput.Close();

            processOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        return processOutput;
    }
}