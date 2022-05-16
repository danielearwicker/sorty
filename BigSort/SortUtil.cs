namespace BigSort;

using System.Threading.Channels;

public static class SortUtil
{
    public static void GenerateWholeFile()
    {
        var keys = new int[60_000_000];

        for (var n = 0; n < keys.Length; n++)
        {
            keys[n] = n;
        }

        var rand = new Random();

        using var writer = FileUtil.OpenWriter(FileUtil.WholeFilePath);

        for (var n = 0; n < keys.Length - 1; n++)
        {
            var pos = rand.Next(n, keys.Length);

            var key = keys[pos];
            keys[pos] = keys[n];

            new Record(
                $"{key:X8}",
                @$"This is the pretend value for key {key} or in hex format {key:X8}, and I'm now
              just padding it out to make sure my file is large enough to be realistic.")
                .Write(writer);

            Logger.Log($"Generated {n}...", true);
        }
    }

    private static Task StartReading(ChannelWriter<(Record[], int)> writer, ChannelReader<Record[]> buffers) => Task.Run(async () =>
        {
            using var wholeFileReader = FileUtil.OpenReader(FileUtil.WholeFilePath);

            while (true)
            {
                var buffer = await buffers.ReadAsync();

                var actualSize = Record.ReadChunk(wholeFileReader, buffer);
                if (actualSize == 0)
                {
                    break;
                }

                await writer.WriteAsync((buffer, actualSize));
            }

            await writer.WriteAsync((Array.Empty<Record>(), 0));
        });


    private static Task StartWriting(ChannelReader<(int, Record[], int)> reader, ChannelWriter<Record[]> buffers) => Task.Run(async () =>
        {
            while (true)
            {
                var (chunk, buffer, actualSize) = await reader.ReadAsync();
                if (actualSize == 0)
                {
                    break;
                }

                using var chunkWriter = FileUtil.OpenWriter(FileUtil.GetChunkPath(chunk));
                Record.WriteChunk(chunkWriter, buffer, actualSize);

                await buffers.WriteAsync(buffer);
            }
        });

    public static async Task<int> SortChunks(int chunkSize, Action<Record[], int, IComparer<Record>> sort)
    {        
        var chunks = 0;

        var read = Channel.CreateBounded<(Record[] Buffer, int Size)>(1);
        var write = Channel.CreateBounded<(int Chunk, Record[] Buffer, int Size)>(1);
        var buffers = Channel.CreateBounded<Record[]>(3);
        for (var b = 0; b < 3; b++)
        {
            await buffers.Writer.WriteAsync(new Record[chunkSize]);
        }
        
        var reader = StartReading(read.Writer, buffers.Reader);
        var writer = StartWriting(write.Reader, buffers.Writer);
        
        for (; ; chunks++)
        {
            var (buffer, actualSize) = await read.Reader.ReadAsync();
            if (actualSize == 0) break;

            Logger.Log($"Sorting chunk {chunks} of size {actualSize}", false);

            sort(buffer, actualSize, RecordComparer.Instance);

            await write.Writer.WriteAsync((chunks, buffer, actualSize));
        }

        await reader;

        await write.Writer.WriteAsync((-1, Array.Empty<Record>(), 0));
        await writer;

        return chunks;
    }

    public static void MergeChunks(int chunkCount)
    {
        var chunkReaders = new SortedSet<ChunkReader>(
            Enumerable.Range(0, chunkCount).Select(x => new ChunkReader(x)),
            ChunkReaderComparer.Instance);

        using var sortedFileWriter = FileUtil.OpenWriter(FileUtil.SortedFilePath);

        var sortedCount = 0;

        for (; chunkReaders.Count != 0; sortedCount++)
        {
            var next = chunkReaders.First();
            chunkReaders.Remove(next);

            next.Current.Write(sortedFileWriter);
            if (next.Advance())
            {
                chunkReaders.Add(next);
            }

            Logger.Log($"Merged {sortedCount}", true);
        }
    }
}
