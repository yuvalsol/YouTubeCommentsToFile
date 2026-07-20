using CommandLine;

namespace YouTubeCommentsToFile;

internal static class OptionsHelper
{
    public record OptionPrecedence(
        string OptionName,
        string ShortName,
        string LongName,
        string OptionShortName,
        string OptionLongName,
        int Index)
    {
        public static readonly OptionPrecedence Default = new(null, null, null, null, null, int.MaxValue);
    }

    private static OptionPrecedence[] optionsPrecedence;
    public static OptionPrecedence[] OptionsPrecedence
    {
        get
        {
            var baseInterface = typeof(IYouTubeCommentsToFileOptions);
            var derivedInterfaces = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsInterface && t != baseInterface && baseInterface.IsAssignableFrom(t)).ToArray();

            return optionsPrecedence ??= [..
                derivedInterfaces
                .Where(optionsType => derivedInterfaces.HasAny(other => other != optionsType && other.IsAssignableFrom(optionsType)) == false)
                .SelectMany(GetOptionsPrecedence)
                .DistinctBy(x => x.OptionPrecedence) // IConvertOptions and IDownloadOptions share options
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Ordinal)
                .ThenBy(x => x.OptionPrecedence.LongName)
                .Select((x, i) => x.OptionPrecedence with { Index = i })];
        }
    }

    private static IEnumerable<(OptionPrecedence OptionPrecedence, int Group, int Ordinal)> GetOptionsPrecedence(Type optionsType)
    {
        int group = optionsType.GetCustomAttribute<OptionPrecedenceAttribute>()?.Ordinal ?? int.MaxValue;

        return optionsType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(property =>
            (
                OptionName: property.Name,
                optionAttr: property.GetCustomAttribute<OptionAttribute>(),
                optionPrecedenceAttr: property.GetCustomAttribute<OptionPrecedenceAttribute>()
            ))
            .Where(x => x.optionAttr != null)
            .Select(x =>
            (
                optionPrecedence: new OptionPrecedence(
                    x.OptionName,
                    x.optionAttr.ShortName,
                    x.optionAttr.LongName,
                    OptionShortName: string.IsNullOrEmpty(x.optionAttr.ShortName) ? null : $"-{x.optionAttr.ShortName}",
                    OptionLongName: string.IsNullOrEmpty(x.optionAttr.LongName) ? null : $"--{x.optionAttr.LongName}",
                    Index: 0
                ),
                Group: group,
                Ordinal: x.optionPrecedenceAttr?.Ordinal ?? int.MaxValue
            ));
    }

    public static string GetOptionShortName(string optionName)
    {
        return OptionsPrecedence.FirstOrDefault(x => x.OptionName == optionName, OptionPrecedence.Default).OptionShortName;
    }

    public static string GetOptionLongName(string optionName)
    {
        return OptionsPrecedence.FirstOrDefault(x => x.OptionName == optionName, OptionPrecedence.Default).OptionLongName;
    }
}
