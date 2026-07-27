namespace YouTubeCommentsToFile;

internal partial class Program
{
    #region Configuration Stack

    private void AddConfigurationStackToConfigLocations()
    {
        if (operationOptions.ConfigLocations.IsNullOrEmpty() || operationOptions.IgnoreConfig)
            operationOptions.ConfigLocations = GetConfigurationStack();
        else
            operationOptions.ConfigLocations = operationOptions.ConfigLocations.Concat(GetConfigurationStack());

        FixConfigLocations();

        DistinctConfigLocations();

        WriteVerboseConfigurationStack();
    }

    private IEnumerable<string> GetConfigurationStack()
    {
#if LINUX_BUILD
        return
            GetPortableConfiguration()
            .Concat(GetHomeConfiguration())
            .Concat(GetLinuxUserConfiguration())
            .Concat(GetSystemConfiguration());
#elif WINDOWS_BUILD
        return
            GetPortableConfiguration()
            .Concat(GetHomeConfiguration())
            .Concat(GetWindowsUserConfiguration());
#else
        return [];
#endif
    }

    private IEnumerable<string> GetPortableConfiguration()
    {
        return GetConfFiles(AppContext.BaseDirectory);
    }

    private IEnumerable<string> GetHomeConfiguration()
    {
        if (string.IsNullOrEmpty(homeConfigurationDirectory))
            return [];
        return GetConfFiles(homeConfigurationDirectory);
    }

#if LINUX_BUILD

    private IEnumerable<string> GetLinuxUserConfiguration()
    {
        string xdgConfigHome = GetEnvironmentVariable("XDG_CONFIG_HOME");

        // $XDG_CONFIG_HOME/youtube-comments-to-file.conf
        return GetConfFiles(xdgConfigHome).Concat(

        // $XDG_CONFIG_HOME/youtube-comments-to-file/config
        GetConfigFiles(xdgConfigHome, "youtube-comments-to-file")).Concat(

        // $XDG_CONFIG_HOME/youtube-comments-to-file/config.txt
        GetConfigTxtFiles(xdgConfigHome, "youtube-comments-to-file")).Concat(

        // ~/youtube-comments-to-file.conf
        GetConfFiles("~")).Concat(

        // ~/youtube-comments-to-file.conf.txt
        GetConfTxtFiles("~")).Concat(

        // ~/.youtube-comments-to-file/config
        GetConfigFiles("~", ".youtube-comments-to-file")).Concat(

        // ~/.youtube-comments-to-file/config.txt
        GetConfigTxtFiles("~", ".youtube-comments-to-file"));
    }

    private IEnumerable<string> GetSystemConfiguration()
    {
        string etc = Path.Combine("/", "etc");

        // /etc/youtube-comments-to-file.conf
        return GetConfFiles(etc).Concat(

        // /etc/youtube-comments-to-file/config
        GetConfigFiles(etc, "youtube-comments-to-file")).Concat(

        // /etc/youtube-comments-to-file/config.txt
        GetConfigTxtFiles(etc, "youtube-comments-to-file"));
    }

#elif WINDOWS_BUILD

    private IEnumerable<string> GetWindowsUserConfiguration()
    {
        string appData = GetEnvironmentVariable("APPDATA");
        string userProfile = GetEnvironmentVariable("USERPROFILE");

        // %APPDATA%\youtube-comments-to-file.conf
        return GetConfFiles(appData).Concat(

        // %APPDATA%\YouTubeCommentsToFile\config
        GetConfigFiles(appData, "YouTubeCommentsToFile")).Concat(

        // %APPDATA%\YouTubeCommentsToFile\config.txt
        GetConfigTxtFiles(appData, "YouTubeCommentsToFile")).Concat(

        // %USERPROFILE%\youtube-comments-to-file.conf
        GetConfFiles(userProfile)).Concat(

        // %USERPROFILE%\youtube-comments-to-file.conf.txt
        GetConfTxtFiles(userProfile)).Concat(

        // %USERPROFILE%\.youtubecommentstofile\config
        GetConfigFiles(userProfile, ".youtubecommentstofile")).Concat(

        // %USERPROFILE%\.youtubecommentstofile\config.txt
        GetConfigTxtFiles(userProfile, ".youtubecommentstofile"));
    }

#endif

    private const string CONF = "youtube-comments-to-file{0}.conf";
    private const string CONF_TXT = "youtube-comments-to-file{0}.conf.txt";
    private const string CONFIG = "{0}";
    private const string CONFIG_TXT = "{0}.txt";

    private IEnumerable<string> GetConfFileNames() => GetConfigurationFileNames(CONF, $"-{operationOptions.Verb}", string.Empty);
    private IEnumerable<string> GetConfTxtFileNames() => GetConfigurationFileNames(CONF_TXT, $"-{operationOptions.Verb}", string.Empty);
    private IEnumerable<string> GetConfigFileNames() => GetConfigurationFileNames(CONFIG, operationOptions.Verb, "config");
    private IEnumerable<string> GetConfigTxtFileNames() => GetConfigurationFileNames(CONFIG_TXT, operationOptions.Verb, "config");

    private static IEnumerable<string> GetConfigurationFileNames(string template, string f1, string f2)
    {
        yield return string.Format(template, f1);
        yield return string.Format(template, f2);
    }

    private IEnumerable<string> GetConfFiles(string directory) => GetConfigurationFiles(GetConfFileNames, directory);
    private IEnumerable<string> GetConfFiles(string directory1, string directory2) => GetConfigurationFiles(GetConfFileNames, directory1, directory2);

    private IEnumerable<string> GetConfTxtFiles(string directory) => GetConfigurationFiles(GetConfTxtFileNames, directory);
    private IEnumerable<string> GetConfTxtFiles(string directory1, string directory2) => GetConfigurationFiles(GetConfTxtFileNames, directory1, directory2);

    private IEnumerable<string> GetConfigFiles(string directory) => GetConfigurationFiles(GetConfigFileNames, directory);
    private IEnumerable<string> GetConfigFiles(string directory1, string directory2) => GetConfigurationFiles(GetConfigFileNames, directory1, directory2);

    private IEnumerable<string> GetConfigTxtFiles(string directory) => GetConfigurationFiles(GetConfigTxtFileNames, directory);
    private IEnumerable<string> GetConfigTxtFiles(string directory1, string directory2) => GetConfigurationFiles(GetConfigTxtFileNames, directory1, directory2);

    private static IEnumerable<string> GetConfigurationFiles(Func<IEnumerable<string>> GetFileNames, string directory)
    {
        return GetFileNames().Select(file => Path.Combine(directory, file));
    }

    private static IEnumerable<string> GetConfigurationFiles(Func<IEnumerable<string>> GetFileNames, string directory1, string directory2)
    {
        return GetFileNames().Select(file => Path.Combine(directory1, directory2, file));
    }

    #endregion

    #region Environment Variables

    private static string GetEnvironmentVariable(string variable)
    {
        return variable switch
        {
#if LINUX_BUILD
            "~" or "HOME" => "~",
#elif WINDOWS_BUILD
            "~" or "HOME" => GetWindowsHomePath(),
#endif
            "XDG_CONFIG_HOME" => GetEnvironmentVariable("XDG_CONFIG_HOME", ".config"),
            "XDG_CACHE_HOME" => GetEnvironmentVariable("XDG_CACHE_HOME", ".cache"),
            _ => Environment.GetEnvironmentVariable(variable)
        };
    }

    private static string GetEnvironmentVariable(string variable, string fallback)
    {
        string path = Environment.GetEnvironmentVariable(variable);
        return string.IsNullOrEmpty(path) ? Path.Combine(GetEnvironmentVariable("~"), fallback) : path;
    }

    private static string GetWindowsHomePath()
    {
        string path = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(path))
            path = Environment.GetEnvironmentVariable("USERPROFILE");
        if (string.IsNullOrEmpty(path))
            path = $"{Environment.GetEnvironmentVariable("HOMEDRIVE")}{Environment.GetEnvironmentVariable("HOMEPATH")}";
        return path;
    }

    [GeneratedRegex(@"\${(?<Var>.+)}|\$(?<Var>\S+)|%(?<Var>.+)%")]
    private static partial Regex RegexEnvironmentVariable();

    private static string FixPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        path = string.Join(
            Path.DirectorySeparatorChar,
            path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar])
                .Select(s => s == "~" ? GetEnvironmentVariable("~") : s)
        );

        return RegexEnvironmentVariable().Replace(path, match => GetEnvironmentVariable(match.Groups["Var"].Value));
    }

    #endregion

    #region Merge Config Locations

    private ParsingResult MergeConfigLocations()
    {
        if (FixConfigLocations() == false)
            return ParsingResult.Success;

        HashSet<string> encounteredConfigFiles = [];

        while (operationOptions.ConfigLocations.HasAny())
        {
            var (configFile, isConfigFileEncountered) = GetFirstConfigFile(encounteredConfigFiles);
            if (isConfigFileEncountered)
            {
                WriteVerboseConfigFileEncountered(configFile);
                continue;
            }

            var (options, parsingResult) = GetOptionsFromConfigFile(configFile);

            if (parsingResult == ParsingResult.Success)
            {
                if (options.StopConfig)
                {
                    return ParsingResult.Success;
                }
                else if (options.IgnoreConfig == false)
                {
                    if (operationOptions is IOperationOptions<ConvertOptions> convertOperationOptions && options is ConvertOptions convertOptions)
                        convertOperationOptions.MergeOptions(convertOptions);
                    else if (operationOptions is IOperationOptions<DownloadOptions> downloadOperationOptions && options is DownloadOptions downloadOptions)
                        downloadOperationOptions.MergeOptions(downloadOptions);
                    else
                        throw new Exception($"Merging options failed. Command line options and config file options are not the same type. Config file '{configFile}'.");

                    ConcatConfigLocations(options, configFile);
                }
            }
            else if (parsingResult == ParsingResult.Version)
            {
                operationOptions.Version = true;
            }
            else if (parsingResult == ParsingResult.Help)
            {
                operationOptions.Help = true;
            }
            else if (parsingResult == ParsingResult.Error)
            {
                return parsingResult;
            }
        }

        return ParsingResult.Success;
    }

    private bool FixConfigLocations()
    {
        if (operationOptions.ConfigLocations.IsNullOrEmpty())
            return false;

        FixConfigLocations(operationOptions, AppContext.BaseDirectory);
        return true;
    }

    private void FixConfigLocations(IOperationOptions operationOptions, string baseDirectory)
    {
        operationOptions.ConfigLocations =
            operationOptions.ConfigLocations
            .Where(configFile => string.IsNullOrWhiteSpace(configFile) == false)
            .Select(FixPath)
            .Select(configFile => Path.IsPathFullyQualified(configFile) ? configFile : Path.Combine(baseDirectory, configFile))
            .SelectMany(configFile =>
            {
                if (Directory.Exists(configFile))
                    return GetConfFiles(configFile);
                return [configFile];
            })
            .Where(File.Exists);
    }

    private void DistinctConfigLocations()
    {
        if (operationOptions.ConfigLocations.IsNullOrEmpty())
            return;

        operationOptions.ConfigLocations =
            operationOptions.ConfigLocations
            .Select((configFile, i) => (configFile, i))
            .GroupBy(x => x.configFile)
            .Select(g => (configFile: g.Key, i: g.Min(x => x.i)))
            .OrderBy(x => x.i)
            .Select(x => x.configFile);
    }

    private (string configFile, bool isConfigFileEncountered) GetFirstConfigFile(HashSet<string> encounteredConfigFiles)
    {
        string configFile = operationOptions.ConfigLocations.First();
        operationOptions.ConfigLocations = operationOptions.ConfigLocations.Skip(1);

        if (encounteredConfigFiles.Contains(configFile))
            return (configFile, true);

        encounteredConfigFiles.Add(configFile);
        return (configFile, false);
    }

    private (IOperationOptions parsingOptions, ParsingResult parsingResult) GetOptionsFromConfigFile(string configFile)
    {
        string[] args = [.. ReadLines(configFile).SelectMany(line => line.ToArgs())];

        if (args.IsNullOrEmpty())
        {
            WriteVerboseConfigFileNoOptions(configFile);
            return (null, ParsingResult.ArgumentsNotParsed);
        }

        var results = ParseArgs([.. args.Prepend(operationOptions.Verb)]);
        WriteVerboseConfigFile(configFile, args, results.parsingResult);
        return results;
    }

    private static IEnumerable<string> ReadLines(string path)
    {
        try
        {
            return ReadLines(path, Encoding.UTF8);
        }
        catch (DecoderFallbackException)
        {
            return ReadLines(path, Encoding.Default);
        }
    }

    private static IEnumerable<string> ReadLines(string path, Encoding encoding)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return File.ReadLines(path, Encoding.GetEncoding(
            encoding.CodePage,
            EncoderFallback.ExceptionFallback,
            DecoderFallback.ExceptionFallback
        ));
    }

    private void ConcatConfigLocations(IOperationOptions options, string configFile)
    {
        if (options.ConfigLocations.IsNullOrEmpty())
            return;

        FixConfigLocations(options, Path.GetDirectoryName(configFile));
        operationOptions.ConfigLocations = options.ConfigLocations.Concat(operationOptions.ConfigLocations);
    }

    #endregion

    #region Write Verbose Configuration

    private void WriteVerboseHomeConfigurationDirectory()
    {
        if (string.IsNullOrEmpty(homeConfigurationDirectory) == false)
            verboseMessages.Add([.. GetPropertyMessages(nameof(homeConfigurationDirectory).PrettifyName(), [homeConfigurationDirectory])]);
    }

    private void WriteVerboseConfigurationStack()
    {
        if (operationOptions.ConfigLocations.HasAny())
            verboseMessages.Add([.. GetPropertyMessages("Configuration Stack", [.. operationOptions.ConfigLocations])]);
    }

    private void WriteVerboseConfigFileEncountered(string configFile)
    {
        verboseMessages.Add([.. GetPropertyMessages(configFile, ["Ignored. File was loaded previously."], WARNING_COLOR)]);
    }

    private void WriteVerboseConfigFileNoOptions(string configFile)
    {
        verboseMessages.Add([.. GetPropertyMessages(configFile, ["Ignored. No options in file."], WARNING_COLOR)]);
    }

    private void WriteVerboseConfigFile(string configFile, string[] args, ParsingResult parsingResult)
    {
        if (parsingResult == ParsingResult.Error)
            WriteVerboseArgs(configFile, ["Failed to parse options from file.", .. args], ERROR_COLOR);
        else
            WriteVerboseArgs(configFile, args);
    }

    #endregion
}
