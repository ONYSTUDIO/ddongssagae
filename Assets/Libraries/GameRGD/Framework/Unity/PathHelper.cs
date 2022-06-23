using System.IO;

public static class PathHelper
{
    public static string Combine(string path1, string path2)
    {
        return Path.Combine(path1, path2).Replace("\\", "/");
    }

    public static bool ValidatePath(string path)
    {
        return false == string.IsNullOrEmpty(path) && 
            -1 == path.IndexOfAny(Path.GetInvalidPathChars()) && 
            true == Path.HasExtension(path);
    }
}