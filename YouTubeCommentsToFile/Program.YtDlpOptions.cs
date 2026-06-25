namespace YouTubeCommentsToFile;

internal partial class Program
{
    private static void AddQuotationsToYtDlpOptions(string[] args)
    {
        var argYtDlpOptions = args.Select((arg, index) => (arg, index)).FirstOrDefault(x => string.Compare(x.arg, "--yt-dlp-options", StringComparison.OrdinalIgnoreCase) == 0);
        if (argYtDlpOptions == default)
            return;

        int index = argYtDlpOptions.index + 1;
        if (index >= args.Length)
            return;

        string value = args[index];
        if ((value.StartsWith('"') && value.EndsWith('"')) == false)
            args[index] = $@"""{value}""";
    }

    private static void RemoveQuotationsFromYtDlpOptions(DownloadOptions options)
    {
        if (string.IsNullOrEmpty(options.YtDlpOptions))
            return;

        if (options.YtDlpOptions.StartsWith('"') && options.YtDlpOptions.EndsWith('"'))
            options.YtDlpOptions = options.YtDlpOptions[1..^1];
    }
}
