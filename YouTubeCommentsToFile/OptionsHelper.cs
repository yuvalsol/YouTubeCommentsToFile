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
                .DistinctBy(x => x.OptionPrecedence)
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Ordinal)
                .ThenBy(x => x.OptionPrecedence.LongName)
                .Select((x, i) => x.OptionPrecedence with { Index = i })];
        }
    }

    private static IEnumerable<(OptionPrecedence OptionPrecedence, int Group, int Ordinal)> GetOptionsPrecedence(Type optionsType)
    {
        return
            GetPrecedence(optionsType)
            .Select(x =>
            (
                new OptionPrecedence(
                    x.OptionName,
                    x.ShortName,
                    x.LongName,
                    x.OptionShortName,
                    x.OptionLongName,
                    Index: 0
                ),
                x.Group,
                x.Ordinal
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

    public record OptionPrecedenceWithValue(
        string OptionName,
        object OptionValue,
        string ShortName,
        string LongName,
        string OptionShortName,
        string OptionLongName,
        int Index
        ) : OptionPrecedence(
            OptionName,
            ShortName,
            LongName,
            OptionShortName,
            OptionLongName,
            Index)
    {
        public IEnumerable<string> ToArgs()
        {
            yield return string.IsNullOrEmpty(OptionLongName) ? OptionShortName : OptionLongName;

            if (OptionValue is bool)
            {
                yield break;
            }
            else if (OptionValue is IEnumerable<string> values)
            {
                foreach (var val in values)
                    yield return val;
            }
            else
            {
                yield return OptionValue.ToString();
            }
        }

        public override string ToString()
        {
            string option = string.IsNullOrEmpty(OptionLongName) ? OptionShortName : OptionLongName;

            if (OptionValue is bool)
                return option;
            else if (OptionValue is IEnumerable<string> values)
                return $"{option} {string.Join(' ', values.Select(val => val.FormatValue()))}";
            else
                return $"{option} {OptionValue.ToString().FormatValue()}";
        }
    }

    public static OptionPrecedenceWithValue[] GetInstanceOptionsPrecedence(object obj)
    {
        var baseInterface = typeof(IYouTubeCommentsToFileOptions);
        var derivedInterfaces = obj.GetType().GetInterfaces().Where(t => t != baseInterface && baseInterface.IsAssignableFrom(t)).ToArray();

        return [..
            derivedInterfaces
            .Where(optionsType => derivedInterfaces.HasAny(other => other != optionsType && other.IsAssignableFrom(optionsType)) == false)
            .SelectMany(optionsType => GetInstanceOptionsPrecedence(optionsType, obj))
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Ordinal)
            .ThenBy(x => x.OptionPrecedence.LongName)
            .Select((x, i) => x.OptionPrecedence with { Index = i })];
    }

    private static IEnumerable<(OptionPrecedenceWithValue OptionPrecedence, int Group, int Ordinal)> GetInstanceOptionsPrecedence(Type optionsType, object obj)
    {
        return
            GetPrecedence(optionsType, obj)
            .Where(x =>
                (x.OptionValue == null ||
                (x.OptionValue is bool val && val == false) ||
                (x.OptionValue is string str && string.IsNullOrEmpty(str)) ||
                (x.OptionValue is IEnumerable<string> values && values.IsNullOrEmpty())) == false)
            .Select(x =>
            (
                new OptionPrecedenceWithValue(
                    x.OptionName,
                    x.OptionValue,
                    x.ShortName,
                    x.LongName,
                    x.OptionShortName,
                    x.OptionLongName,
                    Index: 0
                ),
                x.Group,
                x.Ordinal
            ));
    }

    public static IEnumerable<(
        string OptionName,
        object OptionValue,
        string ShortName,
        string LongName,
        string OptionShortName,
        string OptionLongName,
        int Group,
        int Ordinal)>
        GetPrecedence(Type optionsType, object obj = null)
    {
        int group = optionsType.GetCustomAttribute<OptionPrecedenceAttribute>()?.Ordinal ?? int.MaxValue;

        return optionsType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(property =>
            (
                OptionName: property.Name,
                OptionValue: obj != null ? property.GetValue(obj) : null,
                optionAttr: property.GetCustomAttribute<OptionAttribute>(),
                optionPrecedenceAttr: property.GetCustomAttribute<OptionPrecedenceAttribute>()
            ))
            .Where(x => x.optionAttr != null)
            .Select(x =>
            (
                x.OptionName,
                x.OptionValue,
                x.optionAttr.ShortName,
                x.optionAttr.LongName,
                OptionShortName: string.IsNullOrEmpty(x.optionAttr.ShortName) ? null : $"-{x.optionAttr.ShortName}",
                OptionLongName: string.IsNullOrEmpty(x.optionAttr.LongName) ? null : $"--{x.optionAttr.LongName}",
                Group: group,
                Ordinal: x.optionPrecedenceAttr?.Ordinal ?? int.MaxValue
            ));
    }
}
