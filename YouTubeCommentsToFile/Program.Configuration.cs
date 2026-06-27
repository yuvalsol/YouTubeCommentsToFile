using CommandLine;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static ParsingResult MergeConfigLocations(ConvertOptions convertOptions, Parser parser, bool verbose)
    {
        if (convertOptions.ConfigLocations.IsNullOrEmpty())
            return ParsingResult.Success;

        convertOptions.ConfigLocations = FixConfigLocations(convertOptions.ConfigLocations, AppContext.BaseDirectory);

        HashSet<string> encounteredConfigFiles = [];

        while (convertOptions.ConfigLocations.HasAny())
        {
            string configFile = convertOptions.ConfigLocations.First();

            if (encounteredConfigFiles.Contains(configFile))
                continue;
            encounteredConfigFiles.Add(configFile);

            ParsingResult parsingResult = GetOptionsFromConfigFile(configFile, encounteredConfigFiles, parser, out ConvertOptions options, verbose);

            if (parsingResult == ParsingResult.Success)
            {
                convertOptions.MergeOptions(options);

                convertOptions.ConfigLocations = convertOptions.ConfigLocations.Skip(1);

                if (options.ConfigLocations.HasAny())
                {
                    convertOptions.ConfigLocations =
                        FixConfigLocations(options.ConfigLocations, Path.GetDirectoryName(configFile))
                        .Concat(convertOptions.ConfigLocations);
                }
            }
            else if (parsingResult == ParsingResult.ArgumentsNotParsed)
            {
                continue;
            }
            else
            {
                return parsingResult;
            }
        }

        return ParsingResult.Success;
    }

    private static ParsingResult MergeConfigLocations(DownloadOptions downloadOptions, Parser parser, bool verbose)
    {
        if (downloadOptions.ConfigLocations.IsNullOrEmpty())
            return ParsingResult.Success;

        downloadOptions.ConfigLocations = FixConfigLocations(downloadOptions.ConfigLocations, AppContext.BaseDirectory);

        HashSet<string> encounteredConfigFiles = [];

        while (downloadOptions.ConfigLocations.HasAny())
        {
            string configFile = downloadOptions.ConfigLocations.First();

            if (encounteredConfigFiles.Contains(configFile))
                continue;
            encounteredConfigFiles.Add(configFile);

            ParsingResult parsingResult = GetOptionsFromConfigFile(configFile, encounteredConfigFiles, parser, out DownloadOptions options, verbose);

            if (parsingResult == ParsingResult.Success)
            {
                downloadOptions.MergeOptions(options);

                downloadOptions.ConfigLocations = downloadOptions.ConfigLocations.Skip(1);

                if (options.ConfigLocations.HasAny())
                {
                    downloadOptions.ConfigLocations =
                        FixConfigLocations(options.ConfigLocations, Path.GetDirectoryName(configFile))
                        .Concat(downloadOptions.ConfigLocations);
                }
            }
            else if (parsingResult == ParsingResult.ArgumentsNotParsed)
            {
                continue;
            }
            else
            {
                return parsingResult;
            }
        }

        return ParsingResult.Success;
    }

    private static IEnumerable<string> FixConfigLocations(IEnumerable<string> configLocations, string baseDirectory)
    {
        return
            configLocations
            .Select(configFile => FixConfigFile(ref configFile, baseDirectory) ? configFile : null)
            .Where(configFile => configFile != null);
    }

    private static bool FixConfigFile(ref string configFile, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(configFile))
            return false;

        if (Path.IsPathFullyQualified(configFile) == false)
            configFile = Path.Combine(baseDirectory, configFile);

        if (Directory.Exists(configFile))
            configFile = Path.Combine(configFile, CONF);

        return File.Exists(configFile);
    }

    private static ParsingResult GetOptionsFromConfigFile(string configFile, HashSet<string> encounteredConfigFiles, Parser parser, out ConvertOptions convertOptions, bool verbose)
    {
        var args = GetArgsFromConfigFile(configFile, encounteredConfigFiles);

        if (args.IsNullOrEmpty())
        {
            convertOptions = null;
            return ParsingResult.ArgumentsNotParsed;
        }

        ParsingResult parsingResult = ParseArgs(parser, [.. args.Prepend(ConvertOptions.Verb)], out convertOptions);

        WriteVerboseConfigFile(configFile, args, parsingResult, verbose);

        return parsingResult;
    }

    private static ParsingResult GetOptionsFromConfigFile(string configFile, HashSet<string> encounteredConfigFiles, Parser parser, out DownloadOptions downloadOptions, bool verbose)
    {
        var args = GetArgsFromConfigFile(configFile, encounteredConfigFiles);

        if (args.IsNullOrEmpty())
        {
            downloadOptions = null;
            return ParsingResult.ArgumentsNotParsed;
        }

        ParsingResult parsingResult = ParseArgs(parser, [.. args.Prepend(DownloadOptions.Verb)], out downloadOptions);

        WriteVerboseConfigFile(configFile, args, parsingResult, verbose);

        return parsingResult;
    }

    [GeneratedRegex(@"^\s*(?<Option>-[A-Za-z0-9]|--[A-Za-z0-9][A-Za-z0-9-]*)(?:\s*$|\s+(?:""(?<Value>.*?)""|(?<Value>.*?))\s*$)")]
    private static partial Regex RegexOption();

    private static IEnumerable<string> GetArgsFromConfigFile(string configFile, HashSet<string> encounteredConfigFiles)
    {
        var args =
            File.ReadLines(configFile)
            .Select(line => RegexOption().Match(line))
            .Where(match => match.Success)
            .SelectMany(match =>
            {
                if (match.Groups["Value"].Success)
                    return [match.Groups["Option"].Value, match.Groups["Value"].Value];
                return new string[] { match.Groups["Option"].Value };
            });

        foreach (var arg in args)
            yield return arg;
    }

    private static void WriteVerboseConfigFile(string configFile, IEnumerable<string> args, ParsingResult parsingResult, bool verbose)
    {
        if (verbose)
        {
            if (parsingResult == ParsingResult.Success)
                WritePropertyLines(configFile, RearrangeArgs([.. args]));
            else
                WritePropertyLines(configFile, Color.Red, RearrangeArgs([.. args]));
        }
    }

    private static void AddConfigurationStackToConfigLocations(IConfigurationOptions options, string homeConfigurationDirectory)
    {
        if (options.ConfigLocations.IsNullOrEmpty())
            options.ConfigLocations = GetConfigurationStack(homeConfigurationDirectory);
        else
            options.ConfigLocations = options.ConfigLocations.Concat(GetConfigurationStack(homeConfigurationDirectory));
    }

    private const string CONF = "youtube-comments-to-file.conf";
    private const string CONF_TXT = "youtube-comments-to-file.conf.txt";
    private const string CONFIG = "config";
    private const string CONFIG_TXT = "config.txt";

    private static IEnumerable<string> GetConfigurationStack(string homeConfigurationDirectory)
    {
        yield return GetPortableConfiguration();

        if (string.IsNullOrEmpty(homeConfigurationDirectory) == false)
            yield return GetHomeConfiguration(homeConfigurationDirectory);

#if LINUX_BUILD

        foreach (var path in GetLinuxUserConfiguration())
            yield return path;

        foreach (var path in GetSystemConfiguration())
            yield return path;

#elif WINDOWS_BUILD

        foreach (var path in GetWindowsUserConfiguration())
            yield return path;

#endif
    }

    private static string GetPortableConfiguration()
    {
        return Path.Combine(AppContext.BaseDirectory, CONF);
    }

    private static string GetHomeConfiguration(string homeConfigurationDirectory)
    {
        return Path.Combine(homeConfigurationDirectory, CONF);
    }

#if LINUX_BUILD

    private static IEnumerable<string> GetLinuxUserConfiguration()
    {
        string homeDir = GetUserProfilePath();
        string xdgConfigHome = GetXdgConfigHomePath(homeDir);

        // $XDG_CONFIG_HOME/youtube-comments-to-file.conf
        yield return Path.Combine(xdgConfigHome, CONF);

        // $XDG_CONFIG_HOME/youtube-comments-to-file/config
        yield return Path.Combine(xdgConfigHome, "youtube-comments-to-file", CONFIG);

        // $XDG_CONFIG_HOME/youtube-comments-to-file/config.txt
        yield return Path.Combine(xdgConfigHome, "youtube-comments-to-file", CONFIG_TXT);

        // ~/youtube-comments-to-file.conf
        yield return Path.Combine(homeDir, CONF);

        // ~/youtube-comments-to-file.conf.txt
        yield return Path.Combine(homeDir, CONF_TXT);

        // ~/.youtube-comments-to-file/config
        yield return Path.Combine(homeDir, ".youtube-comments-to-file", CONFIG);

        // ~/.youtube-comments-to-file/config.txt
        yield return Path.Combine(homeDir, ".youtube-comments-to-file", CONFIG_TXT);
    }

    private static IEnumerable<string> GetSystemConfiguration()
    {
        string etc = GetEtcPath();

        // /etc/youtube-comments-to-file.conf
        yield return Path.Combine(etc, CONF);

        // /etc/youtube-comments-to-file/config
        yield return Path.Combine(etc, "youtube-comments-to-file", CONFIG);

        // /etc/youtube-comments-to-file/config.txt
        yield return Path.Combine(etc, "youtube-comments-to-file", CONFIG_TXT);
    }

    private static string GetXdgConfigHomePath(string homeDir)
    {
        string XDG_CONFIG_HOME = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrWhiteSpace(XDG_CONFIG_HOME))
            return Path.Combine(homeDir, ".config"); // fallback to ~/.config
        return XDG_CONFIG_HOME;
    }

    private static string GetEtcPath()
    {
        return Path.Combine("/", "etc");
    }

#elif WINDOWS_BUILD

    private static IEnumerable<string> GetWindowsUserConfiguration()
    {
        string appData = GetAppDataPath();

        // %APPDATA%\youtube-comments-to-file.conf
        yield return Path.Combine(appData, CONF);

        // %APPDATA%\YouTubeCommentsToFile\config
        yield return Path.Combine(appData, "YouTubeCommentsToFile", CONFIG);

        // %APPDATA%\YouTubeCommentsToFile\config.txt
        yield return Path.Combine(appData, "YouTubeCommentsToFile", CONFIG_TXT);

        string userProfile = GetUserProfilePath();

        // %USERPROFILE%\youtube-comments-to-file.conf
        yield return Path.Combine(userProfile, CONF);

        // %USERPROFILE%\youtube-comments-to-file.conf.txt
        yield return Path.Combine(userProfile, CONF_TXT);

        // %USERPROFILE%\.youtubecommentstofile\config
        yield return Path.Combine(userProfile, ".youtubecommentstofile", CONFIG);

        // %USERPROFILE%\.youtubecommentstofile\config.txt
        yield return Path.Combine(userProfile, ".youtubecommentstofile", CONFIG_TXT);
    }

    private static string GetAppDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

#endif

    private static string GetUserProfilePath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
