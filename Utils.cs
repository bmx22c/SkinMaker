class Utils
{
    public void ExitWithMessage(string message){
        Console.WriteLine(message);
        Console.Write("Press any key to close..."); Console.ReadLine();
        Environment.Exit(0);
    }
}