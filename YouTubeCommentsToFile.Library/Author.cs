namespace YouTubeCommentsToFile.Library;

internal sealed record Author(string Name, int Index, int Length)
{
    public AtSigns AtSigns { get; init; }
    public FalseStart FalseStart { get; init; }

    public bool Equals(Author other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return
            Name == other.Name &&
            Index == other.Index &&
            Length == other.Length;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Index, Length);
}

internal sealed record AtSigns(int Index, int Length);
internal sealed record FalseStart(int Index, int Length);
