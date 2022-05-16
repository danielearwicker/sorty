namespace BigSort;

public static class FileUtil
{
    public const string homeDir = "/Users/danielearwicker/src/jim/data";

    public static string GetHomePath(string file) => Path.Combine(homeDir, file)!;

    public static readonly string WholeFilePath = GetHomePath("wholeFile");

    public static readonly string SortedFilePath = GetHomePath("sortedFile");

    public static string GetChunkPath(int n) => GetHomePath($"chunk-{n}");

    public static BinaryWriter OpenWriter(string path)
        => new(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize));

    public static BinaryReader OpenReader(string path)
        => new(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize));

    public static int BufferSize = 1_000_000;
}
