using CommandLine;

namespace YouTubeCommentsToFile;

internal static class OptionsHelper
{
    public static string ConvertVerb => GetVerbValue<ConvertOptions, string>("Name");
    public static bool ConvertIsDefaultVerb => GetVerbValue<ConvertOptions, bool>("IsDefault");
    public static string ConvertHelpText => GetVerbValue<ConvertOptions, string>("HelpText");
    public static string DownloadVerb => GetVerbValue<DownloadOptions, string>("Name");
    public static bool DownloadIsDefaultVerb => GetVerbValue<DownloadOptions, bool>("IsDefault");
    public static string DownloadHelpText => GetVerbValue<DownloadOptions, string>("HelpText");

    private static TReturn GetVerbValue<TOptions, TReturn>(string propertyName)
        where TOptions : class
    {
        PropertyInfo property = typeof(VerbAttribute).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
        var attribute = typeof(TOptions).GetCustomAttribute<VerbAttribute>();
        return (TReturn)property.GetValue(attribute);
    }

    public static string[] GetOptionsPrecedence()
    {
        return [..
            GetOptionsPrecedence<IConvertOptions>()
            .Concat(GetOptionsPrecedence<IDownloadOptions>())
            .Concat(GetOptionsPrecedence<ISharedOptions>())
            .Concat(GetOptionsPrecedence<IUploaderOptions>())
            .Concat(GetOptionsPrecedence<IAuthorsOptions>())
            .Concat(GetOptionsPrecedence<ITextsOptions>())
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

    public static string GetOptionLongName(string settingName)
    {
        return
            GetLongNames<IConvertOptions>()
            .Concat(GetLongNames<IDownloadOptions>())
            .Concat(GetLongNames<ISharedOptions>())
            .Concat(GetLongNames<IUploaderOptions>())
            .Concat(GetLongNames<IAuthorsOptions>())
            .Concat(GetLongNames<ITextsOptions>())
            .FirstOrDefault(x => x.Name == settingName).LongName;
    }

    private static IEnumerable<(string Name, string LongName)> GetLongNames<IOptions>()
    {
        return typeof(IOptions)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(property =>
            (
                property.Name,
                property.GetCustomAttribute<OptionAttribute>().LongName
            ));
    }
}
