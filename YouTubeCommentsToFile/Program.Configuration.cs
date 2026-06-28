using CommandLine;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    #region Configuration Stack

    private static void AddConfigurationStackToConfigLocations(IOperationOptions operationOptions, string homeConfigurationDirectory, bool verbose)
    {
        if (operationOptions.StopConfig)
        {
            operationOptions.ConfigLocations = null;
            return;
        }

        if (operationOptions.ConfigLocations.IsNullOrEmpty() || operationOptions.IgnoreConfig)
            operationOptions.ConfigLocations = GetConfigurationStack(homeConfigurationDirectory, operationOptions.Verb);
        else
            operationOptions.ConfigLocations = operationOptions.ConfigLocations.Concat(GetConfigurationStack(homeConfigurationDirectory, operationOptions.Verb));

        FixConfigLocations(operationOptions);

        DistinctConfigLocations(operationOptions);

        WriteVerboseConfigurationStack(operationOptions, verbose);
    }

    private static IEnumerable<string> GetConfigurationStack(string homeConfigurationDirectory, string verb)
    {
#if LINUX_BUILD

        return
            GetPortableConfiguration(verb)
            .Concat(GetHomeConfiguration(homeConfigurationDirectory, verb))
            .Concat(GetLinuxUserConfiguration(verb))
            .Concat(GetSystemConfiguration(verb));

#elif WINDOWS_BUILD

        return
            GetPortableConfiguration(verb)
            .Concat(GetHomeConfiguration(homeConfigurationDirectory, verb))
            .Concat(GetWindowsUserConfiguration(verb));

#endif
    }

    private static IEnumerable<string> GetPortableConfiguration(string verb)
    {
        return GetConfFiles(AppContext.BaseDirectory, verb);
    }

    private static IEnumerable<string> GetHomeConfiguration(string homeConfigurationDirectory, string verb)
    {
        if (string.IsNullOrEmpty(homeConfigurationDirectory))
            return [];
        return GetConfFiles(homeConfigurationDirectory, verb);
    }

#if LINUX_BUILD

    private static IEnumerable<string> GetLinuxUserConfiguration(string verb)
    {
        string xdgConfigHome = GetEnvironmentVariable("XDG_CONFIG_HOME");

        // $XDG_CONFIG_HOME/youtube-comments-to-file.conf
        return GetConfFiles(xdgConfigHome, verb).Concat(

        // $XDG_CONFIG_HOME/youtube-comments-to-file/config
        GetConfigFiles(xdgConfigHome, "youtube-comments-to-file", verb)).Concat(

        // $XDG_CONFIG_HOME/youtube-comments-to-file/config.txt
        GetConfigTxtFiles(xdgConfigHome, "youtube-comments-to-file", verb)).Concat(

        // ~/youtube-comments-to-file.conf
        GetConfFiles("~", verb)).Concat(

        // ~/youtube-comments-to-file.conf.txt
        GetConfTxtFiles("~", verb)).Concat(

        // ~/.youtube-comments-to-file/config
        GetConfigFiles("~", ".youtube-comments-to-file", verb)).Concat(

        // ~/.youtube-comments-to-file/config.txt
        GetConfigTxtFiles("~", ".youtube-comments-to-file", verb));
    }

    private static IEnumerable<string> GetSystemConfiguration(string verb)
    {
        string etc = Path.Combine("/", "etc");

        // /etc/youtube-comments-to-file.conf
        return GetConfFiles(etc, verb).Concat(

        // /etc/youtube-comments-to-file/config
        GetConfigFiles(etc, "youtube-comments-to-file", verb)).Concat(

        // /etc/youtube-comments-to-file/config.txt
        GetConfigTxtFiles(etc, "youtube-comments-to-file", verb));
    }

#elif WINDOWS_BUILD

    private static IEnumerable<string> GetWindowsUserConfiguration(string verb)
    {
        string appData = GetEnvironmentVariable("APPDATA");
        string userProfile = GetEnvironmentVariable("USERPROFILE");

        // %APPDATA%\youtube-comments-to-file.conf
        return GetConfFiles(appData, verb).Concat(

        // %APPDATA%\YouTubeCommentsToFile\config
        GetConfigFiles(appData, "YouTubeCommentsToFile", verb)).Concat(

        // %APPDATA%\YouTubeCommentsToFile\config.txt
        GetConfigTxtFiles(appData, "YouTubeCommentsToFile", verb)).Concat(

        // %USERPROFILE%\youtube-comments-to-file.conf
        GetConfFiles(userProfile, verb)).Concat(

        // %USERPROFILE%\youtube-comments-to-file.conf.txt
        GetConfTxtFiles(userProfile, verb)).Concat(

        // %USERPROFILE%\.youtubecommentstofile\config
        GetConfigFiles(userProfile, ".youtubecommentstofile", verb)).Concat(

        // %USERPROFILE%\.youtubecommentstofile\config.txt
        GetConfigTxtFiles(userProfile, ".youtubecommentstofile", verb));
    }

#endif

    private const string CONF = "youtube-comments-to-file{0}.conf";
    private const string CONF_TXT = "youtube-comments-to-file{0}.conf.txt";
    private const string CONFIG = "{0}";
    private const string CONFIG_TXT = "{0}.txt";

    private static IEnumerable<string> GetConfFiles(string directory, string verb)
    {
        return GetConfFileNames(verb).Select(file => Path.Combine(directory, file));
    }

    private static IEnumerable<string> GetConfFiles(string directory1, string directory2, string verb)
    {
        return GetConfFileNames(verb).Select(file => Path.Combine(directory1, directory2, file));
    }

    private static IEnumerable<string> GetConfFileNames(string verb)
    {
        yield return string.Format(CONF, $"-{verb}");
        yield return string.Format(CONF, string.Empty);
    }

    private static IEnumerable<string> GetConfTxtFiles(string directory, string verb)
    {
        return GetConfTxtFileNames(verb).Select(file => Path.Combine(directory, file));
    }

    private static IEnumerable<string> GetConfTxtFiles(string directory1, string directory2, string verb)
    {
        return GetConfTxtFileNames(verb).Select(file => Path.Combine(directory1, directory2, file));
    }

    private static IEnumerable<string> GetConfTxtFileNames(string verb)
    {
        yield return string.Format(CONF_TXT, $"-{verb}");
        yield return string.Format(CONF_TXT, string.Empty);
    }

    private static IEnumerable<string> GetConfigFiles(string directory, string verb)
    {
        return GetConfigFileNames(verb).Select(file => Path.Combine(directory, file));
    }

    private static IEnumerable<string> GetConfigFiles(string directory1, string directory2, string verb)
    {
        return GetConfigFileNames(verb).Select(file => Path.Combine(directory1, directory2, file));
    }

    private static IEnumerable<string> GetConfigFileNames(string verb)
    {
        yield return string.Format(CONFIG, verb);
        yield return string.Format(CONFIG, "config");
    }

    private static IEnumerable<string> GetConfigTxtFiles(string directory, string verb)
    {
        return GetConfigTxtFileNames(verb).Select(file => Path.Combine(directory, file));
    }

    private static IEnumerable<string> GetConfigTxtFiles(string directory1, string directory2, string verb)
    {
        return GetConfigTxtFileNames(verb).Select(file => Path.Combine(directory1, directory2, file));
    }

    private static IEnumerable<string> GetConfigTxtFileNames(string verb)
    {
        yield return string.Format(CONFIG_TXT, verb);
        yield return string.Format(CONFIG_TXT, "config");
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

    private static ParsingResult MergeConfigLocations(IOperationOptions operationOptions, Parser parser, bool verbose)
    {
        if (FixConfigLocations(operationOptions))
            return ParsingResult.Success;

        HashSet<string> encounteredConfigFiles = [];

        while (operationOptions.ConfigLocations.HasAny())
        {
            var (configFile, isConfigFileEncountered) = GetFirstConfigFile(operationOptions, encounteredConfigFiles);
            if (isConfigFileEncountered)
            {
                WriteVerboseConfigFileEncountered(configFile, verbose);
                continue;
            }

            var (options, parsingResult) = GetOptionsFromConfigFile(configFile, parser, operationOptions.Verb, verbose);

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
                        throw new Exception($"Merging options failed. Config file '{configFile}'. Parameters '{nameof(operationOptions)}' (main options) and '{nameof(options)}' (config file options) are not the same type.");

                    ConcatConfigLocations(operationOptions, options, configFile);
                }
            }
            else if (parsingResult == ParsingResult.Error)
            {
                return parsingResult;
            }
        }

        return ParsingResult.Success;
    }

    private static bool FixConfigLocations(IOperationOptions operationOptions)
    {
        if (operationOptions.ConfigLocations.IsNullOrEmpty())
            return true;

        FixConfigLocations(operationOptions, AppContext.BaseDirectory);
        return false;
    }

    private static void FixConfigLocations(IOperationOptions operationOptions, string baseDirectory)
    {
        operationOptions.ConfigLocations =
            operationOptions.ConfigLocations
            .Where(configFile => string.IsNullOrWhiteSpace(configFile) == false)
            .Select(FixPath)
            .Select(configFile => Path.IsPathFullyQualified(configFile) ? configFile : Path.Combine(baseDirectory, configFile))
            .SelectMany(configFile =>
            {
                if (Directory.Exists(configFile))
                    return GetConfFiles(configFile, operationOptions.Verb);
                return [configFile];
            })
            .Where(File.Exists);
    }

    private static void DistinctConfigLocations(IOperationOptions operationOptions)
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

    private static (string configFile, bool isConfigFileEncountered) GetFirstConfigFile(IOperationOptions operationOptions, HashSet<string> encounteredConfigFiles)
    {
        string configFile = operationOptions.ConfigLocations.First();
        operationOptions.ConfigLocations = operationOptions.ConfigLocations.Skip(1);

        if (encounteredConfigFiles.Contains(configFile))
            return (configFile, true);

        encounteredConfigFiles.Add(configFile);
        return (configFile, false);
    }

    private static (IOperationOptions operationOptions, ParsingResult parsingResult) GetOptionsFromConfigFile(string configFile, Parser parser, string verb, bool verbose)
    {
        string[] args = [.. GetArgsFromConfigFile(configFile)];

        if (args.IsNullOrEmpty())
        {
            WriteVerboseConfigFileNoOptions(configFile, verbose);
            return (null, ParsingResult.ArgumentsNotParsed);
        }

        var results = ParseArgs(parser, [.. args.Prepend(verb)]);
        WriteVerboseConfigFile(configFile, args, results.parsingResult, verbose);
        return results;
    }

    /* RegexOption:
     *
     * prefix spaces: \s*
     * <Option>:
     *     short name: -[A-Za-z0-9]
     *     single dash, single char
     *     or
     *     long name: --[A-Za-z0-9][A-Za-z0-9-]*
     *     double dash, first char is not dash
     * Values: (
     *     at least one space: \s+
     *     <Value>:
     *         value in quotes: ""(?<Value>.*?)""
     *         or
     *         value not in quotes: (?<Value>[^#\s][^#]*?)
     *         first char is not # or space, rest of the chars are not #
     * )? - if there are any values
     * suffix: \s*(?:#.*)?$
     * spaces or inline comment
     */
    [GeneratedRegex(@"^\s*(?<Option>-[A-Za-z0-9]|--[A-Za-z0-9][A-Za-z0-9-]*)(?:(?:\s+(?:""(?<Value>.*?)""|(?<Value>[^#\s][^#]*?)))+)?\s*(?:#.*)?$")]
    private static partial Regex RegexOption();

    private static IEnumerable<string> GetArgsFromConfigFile(string configFile)
    {
        foreach (var line in ReadLines(configFile))
        {
            Match match = RegexOption().Match(line);
            if (match.Success == false)
                continue;

            yield return match.Groups["Option"].Value;

            Group gValue = match.Groups["Value"];
            if (gValue.Success == false)
                continue;

            foreach (Capture capture in gValue.Captures)
                yield return capture.Value;
        }
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

    private static void ConcatConfigLocations(IOperationOptions operationOptions, IOperationOptions options, string configFile)
    {
        if (options.ConfigLocations.IsNullOrEmpty())
            return;

        FixConfigLocations(options, Path.GetDirectoryName(configFile));
        operationOptions.ConfigLocations = options.ConfigLocations.Concat(operationOptions.ConfigLocations);
    }

    #endregion

    #region Write Verbose Configuration

    private static void WriteVerboseHomeConfigurationDirectory(string homeConfigurationDirectory, bool verbose)
    {
        if (verbose == false)
            return;

        if (string.IsNullOrEmpty(homeConfigurationDirectory) == false)
            WritePropertyLines(nameof(homeConfigurationDirectory).PrettifyName(), homeConfigurationDirectory);
    }

    private static void WriteVerboseConfigurationStack(IOperationOptions operationOptions, bool verbose)
    {
        if (verbose == false)
            return;

        if (operationOptions.ConfigLocations.HasAny())
            WritePropertyLines("Configuration Stack", [.. operationOptions.ConfigLocations]);
    }

    private static void WriteVerboseConfigFileEncountered(string configFile, bool verbose)
    {
        if (verbose)
            WritePropertyLines(configFile, warningColor, "Ignored. File was loaded previously.");
    }

    private static void WriteVerboseConfigFileNoOptions(string configFile, bool verbose)
    {
        if (verbose)
            WritePropertyLines(configFile, warningColor, "Ignored. No options in file.");
    }

    private static void WriteVerboseConfigFile(string configFile, string[] args, ParsingResult parsingResult, bool verbose)
    {
        if (verbose == false)
            return;

        if (parsingResult == ParsingResult.Error)
        {
            WritePropertyLines(configFile, errorColor, "Failed to parse options from file.");
            if (args.HasAny())
                WriteVerboseArgs(configFile, errorColor, args, verbose);
        }
        else
        {
            WriteVerboseArgs(configFile, args, verbose);
        }
    }

    #endregion
}
