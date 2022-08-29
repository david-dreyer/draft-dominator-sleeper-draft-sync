namespace DraftDominatorSleeperLiveDraftSync;

public static class CredentialsHelper
{
    public static Configuration Get()
    {
        string leagueName = GetLeagueName();
        string sleeperUsername = GetSleeperUsername();
        string footballGuysUsername = GetFootballGuysUsername();
        string footballGuysPassword = GetFootballGuysPassword();

        return new Configuration(leagueName, sleeperUsername, footballGuysUsername, footballGuysPassword);
    }

    private static string GetFootballGuysUsername()
    {
        string? footballGuysUsername = null;

        while (string.IsNullOrWhiteSpace(footballGuysUsername))
        {
            Console.Write("Enter Footballguys (Draft Dominator) Username: ");
            footballGuysUsername = Console.ReadLine();
            Console.WriteLine();
        }
        
        return footballGuysUsername;
    }

    private static string GetLeagueName()
    {
        string? leagueName = null;

        while (string.IsNullOrWhiteSpace(leagueName))
        {
            Console.Write("Enter League Name: ");
            leagueName = Console.ReadLine();
            Console.WriteLine();
        }
        
        return leagueName;
    }

    private static string GetSleeperUsername()
    {
        string? sleeperUsername = null;

        while (string.IsNullOrWhiteSpace(sleeperUsername))
        {
            Console.Write("Enter Sleeper Username: ");
            sleeperUsername = Console.ReadLine();
            Console.WriteLine();
        }
        
        return sleeperUsername;
    }

    private static string GetFootballGuysPassword()
    {
        string pw = string.Empty;
        Console.Write("Enter Footballguys (Draft Dominator) Password: ");
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true);
            // Skip if Backspace or Enter is Pressed
            if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
            {
                pw += keyInfo.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (keyInfo.Key == ConsoleKey.Backspace && pw.Length > 0)
                {
                    // Remove last character if Backspace is Pressed
                    pw = pw.Remove(pw.Length - 1, 1);
                    Console.Write("b b");
                }
            }
        } while (keyInfo.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        Console.WriteLine("---------------------------");
        Console.WriteLine();

        return pw;
    }
}