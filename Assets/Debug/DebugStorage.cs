using System.Collections.Generic;

public static class DebugStorage
{
    public static Dictionary<string, string> Entries { get; private set; } = new Dictionary<string, string>();

    public static void Log(string key, object value)
    {
        Entries[key] = value?.ToString() ?? "null";
    }

    public static void Clear(string key)
    {
        if (Entries.ContainsKey(key))
            Entries.Remove(key);
    }

    public static void ClearAll()
    {
        Entries.Clear();
    }
}
