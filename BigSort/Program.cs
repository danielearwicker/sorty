using System.Diagnostics;
using BigSort;

async Task TestSort()
{
    var results = new List<string>();

    var chunkSize = 1_000_000;

    var timer = new Stopwatch();
    timer.Start();

    var chunkCount = await SortUtil.SortChunks(chunkSize, (ar, len, cmp) => Array.Sort(ar, 0, len, cmp));

    var sortTime = timer.Elapsed.TotalSeconds;

    SortUtil.MergeChunks(chunkCount);

    var mergeTime = timer.Elapsed.TotalSeconds;

    results.Add($"{chunkSize}, {Math.Round(sortTime, 3)}, {Math.Round(mergeTime, 3)}, {Math.Round(sortTime + mergeTime, 3)}");

    foreach (var result in results)
    {
        Console.WriteLine(result);
    }
}

async Task TestSearch()
{
    using var reader = FileUtil.OpenReader(FileUtil.SortedFilePath);

    var timer = new Stopwatch();
    timer.Start();

    while (Record.TryRead(reader, out var record))
    {
        if (record.Key == "non_existent")
        {
            Console.WriteLine(record.Value);
            break;
        }
    }

    var searchTime = timer.Elapsed.TotalSeconds;

    Console.WriteLine($"{Math.Round(searchTime, 3)}");
}

await TestSort();