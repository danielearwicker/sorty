namespace BigSort;

public record struct Record(string Key, string Value)
{
    public void Write(BinaryWriter writer)
    {
        writer.Write(Key);
        writer.Write(Value);
    }

    public static bool TryRead(BinaryReader reader, out Record record)
    {
        // The tidy way to detect end of stream is:
        //
        //   (reader.BaseStream.Position == reader.BaseStream.Length)
        //
        // but that extra check is way slower than catching exception!

        try
        {
            record = new(reader.ReadString(), reader.ReadString());
        }
        catch (EndOfStreamException)
        {
            record = default;
            return false;
        }

        return true;
    }

    public static int ReadChunk(BinaryReader reader, Record[] buffer)
    {
        var chunkSize = 0;
        for (; chunkSize < buffer.Length; chunkSize++)
        {
            if (!TryRead(reader, out buffer[chunkSize]))
            {
                break;
            }

            Logger.Log($"Loaded {chunkSize}...", true);
        }

        return chunkSize;
    }

    public static void WriteChunk(BinaryWriter writer, Record[] buffer, int chunkSize)
    {
        for (var n = 0; n < chunkSize; n++)
        {
            buffer[n].Write(writer);

            Logger.Log($"Saved {n}...", true);            
        }
    }
}
