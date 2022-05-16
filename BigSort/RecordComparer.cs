namespace BigSort;

record RecordComparer : IComparer<Record>
{
    public int Compare(Record x, Record y)
        => StringComparer.InvariantCulture.Compare(x.Key, y.Key);

    public static RecordComparer Instance = new();
}
