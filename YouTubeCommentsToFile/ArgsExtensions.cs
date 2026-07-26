namespace YouTubeCommentsToFile;

internal static partial class ArgsExtensions
{
    /* RegexIsOption:
     *
     * short name: -[A-Za-z0-9]
     * single dash, single char
     * or
     * long name: --[A-Za-z0-9][A-Za-z0-9-]*
     * double dash, first char is not dash
     */
    [GeneratedRegex(@"^(?:-[A-Za-z0-9]|--[A-Za-z0-9][A-Za-z0-9-]*)$")]
    private static partial Regex RegexIsOption();

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

    public static IEnumerable<string> RearrangeArgs(this IEnumerable<string> args)
    {
        int counter = 0;

        return args
            .Select((arg, i) => (arg, n: RegexIsOption().IsMatch(arg) ? ++counter : counter))
            .GroupBy(x => x.n)
            .Select(g =>
            (
                arg: string.Join(' ', g.Skip(1).Select(x => x.arg.Contains(' ') ? $@"""{x.arg}""" : x.arg).Prepend(g.First().arg)),
                n: g.Key
            ))
            .OrderBy(x => x.n)
            .Select(x => x.arg);
    }

    public static IEnumerable<string> OrderByPrecedence(this IEnumerable<string> args)
    {
        return args
            .Select(arg =>
            (
                arg,
                match: RegexOption().Match(arg)
            ))
            .Select(x =>
            (
                x.arg,
                isOption: x.match.Success,
                option: (x.match.Success ? x.match.Groups["Option"].Value : null)
            ))
            .OrderBy(x => x.isOption)
            .ThenBy(x => x.isOption ? OptionsHelper.OptionsPrecedence.FirstOrDefault(p => x.option == p.OptionShortName || x.option == p.OptionLongName, OptionsHelper.OptionPrecedence.Default).Index : int.MaxValue)
            .Select(x => x.arg);
    }

    public static IEnumerable<string> ToArgs(this string line)
    {
        Match match = RegexOption().Match(line);
        if (match.Success == false)
            yield break;

        yield return match.Groups["Option"].Value;

        Group gValue = match.Groups["Value"];
        if (gValue.Success == false)
            yield break;

        foreach (Capture capture in gValue.Captures)
            yield return capture.Value;
    }
}
