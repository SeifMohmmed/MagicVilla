namespace MagicVilla_VillaAPI.Services;

public class Logging : ILogging
{
    public void Log(string message, string type)
    {
        if (type == "Error")
        {
            Console.WriteLine("ERROR - " + message);
        }
        else
            Console.WriteLine(message);
    }
}
