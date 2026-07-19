using CommandLine;

namespace YouTubeCommentsToFile;

internal static class OptionsHelper
{
    public static string[] GetOptionsPrecedence()
    {
        return [..
            GetOptionsPrecedence<IConvertOptions>()
            .Concat(GetOptionsPrecedence<IDownloadOptions>())
            .Concat(GetOptionsPrecedence<ISharedOptions>())
            .Concat(GetOptionsPrecedence<IUploaderOptions>())
            .Concat(GetOptionsPrecedence<IAuthorsOptions>())
            .Concat(GetOptionsPrecedence<ITextsOptions>())
            .Concat(GetOptionsPrecedence<IEncodingOptions>())
            .Concat(GetOptionsPrecedence<IHTMLColorsOptions>())
            .Concat(GetOptionsPrecedence<IConfigurationOptions>())
            .Concat(GetOptionsPrecedence<IVerbosityOptions>())
            .Concat(GetOptionsPrecedence<IGeneralOptions>())
            .Distinct()
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Ordinal)
            .Select(x => x.LongName)];
    }

    private static IEnumerable<(string LongName, int Group, int Ordinal)> GetOptionsPrecedence<IOptions>()
    {
        return typeof(IOptions)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(property => new
            {
                optionAttr = property.GetCustomAttribute<OptionAttribute>(),
                optionPrecedenceAttr = property.GetCustomAttribute<OptionPrecedenceAttribute>()
            })
            .Where(x => x.optionAttr != null && x.optionPrecedenceAttr != null)
            .Select(x => (x.optionAttr.LongName, x.optionPrecedenceAttr.Group, x.optionPrecedenceAttr.Ordinal));
    }

    public static string GetOptionShortName(string optionName)
    {
        string shortName = GetOptionNames().FirstOrDefault(x => x.OptionName == optionName).ShortName;
        if (string.IsNullOrEmpty(shortName))
            return null;
        return $"-{shortName}";
    }

    public static string GetOptionLongName(string optionName)
    {
        string longName = GetOptionNames().FirstOrDefault(x => x.OptionName == optionName).LongName;
        if (string.IsNullOrEmpty(longName))
            return null;
        return $"--{longName}";
    }

    public static IEnumerable<(string OptionName, string ShortName, string LongName)> GetOptionNames()
    {
        return
            GetOptionNames<IConvertOptions>()
            .Concat(GetOptionNames<IDownloadOptions>())
            .Concat(GetOptionNames<ISharedOptions>())
            .Concat(GetOptionNames<IUploaderOptions>())
            .Concat(GetOptionNames<IAuthorsOptions>())
            .Concat(GetOptionNames<ITextsOptions>())
            .Concat(GetOptionNames<IEncodingOptions>())
            .Concat(GetOptionNames<IHTMLColorsOptions>())
            .Concat(GetOptionNames<IConfigurationOptions>())
            .Concat(GetOptionNames<IVerbosityOptions>())
            .Concat(GetOptionNames<IGeneralOptions>());
    }

    private static IEnumerable<(string OptionName, string ShortName, string LongName)> GetOptionNames<IOptions>()
    {
        return typeof(IOptions)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(property =>
            (
                OptionName: property.Name,
                property.GetCustomAttribute<OptionAttribute>().ShortName,
                property.GetCustomAttribute<OptionAttribute>().LongName
            ));
    }
}
