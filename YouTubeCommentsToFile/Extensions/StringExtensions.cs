namespace System;

public static partial class StringExtensions
{
    [GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial Regex RegexCamelToWords();

    public static string ToWords(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return RegexCamelToWords().Replace(value, "$1 $2");
    }

    public static string GetName(this string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return fullName;

        int index = fullName.LastIndexOf('.');
        if (index == -1)
            return fullName;

        return fullName[(index + 1)..];
    }

    public static string PrettifyName(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        name = name.ToWords();
        name = $"{char.ToUpper(name[0])}{name[1..]}";
        return name;
    }
}
