namespace BigSort;

public class ChunkReader
{
    private readonly BinaryReader _reader;

    public Record Current;
    public bool HasCurrent;

    public ChunkReader(int chunk)
    {
        _reader = FileUtil.OpenReader(FileUtil.GetChunkPath(chunk));

        Advance();
    }

    public bool Advance()
    {
        HasCurrent = Record.TryRead(_reader, out Current);

        return HasCurrent;
    }
}

public record ChunkReaderComparer : IComparer<ChunkReader>
{
    public int Compare(ChunkReader? x, ChunkReader? y)
        => StringComparer.InvariantCulture.Compare(x?.Current.Key, y?.Current.Key);

    public static readonly ChunkReaderComparer Instance = new();
}
