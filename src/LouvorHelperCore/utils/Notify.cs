using Dumpify;

namespace LouvorHelperCore.Utils;

public static class Notify
{
    public static bool YesNoBox(string message, bool defaultValue = false)
    {
        Notify.Warning(message);
        Notify.Warning("[y/n]");
        string? input = Console.ReadLine();
        if (string.IsNullOrEmpty(input))
            return defaultValue;

        return input.ToLower() switch
        {
            "y" => true,
            "n" => false,
            _ => defaultValue,
        };
    }

    public static void Info(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void Success(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void Warning(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void Error(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void ObjectDump(Object? obj)
    {
        if (obj is null)
        {
            Notify.Error("Object is null");
            return;
        }
        obj.Dump();
    }
}
