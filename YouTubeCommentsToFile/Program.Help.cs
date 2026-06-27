using CommandLine;
using CommandLine.Text;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static StringBuilder GetHelp()
    {
        return GetHelpMessage(
            GetParser(),
            ["--help"]
        );
    }

    private static StringBuilder GetHelpMessage(Parser parser, string[] args)
    {
        string[] optionsPrecedence = OptionsHelper.GetOptionsPrecedence();

        return new StringBuilder()
            .AppendLine(GetVersion())
            .AppendLine()
            .AppendLine(GetHelpText<ConvertHelpOptions>(parser, args, optionsPrecedence, "Convert"))
            .AppendLine(GetHelpText<DownloadHelpOptions>(parser, args, optionsPrecedence, "Download"))
            .AppendLine(GetHelpText<SharedHelpOptions>(parser, args, optionsPrecedence, "Shared Options"))
            .AppendLine(GetHelpText<UploaderHelpOptions>(parser, args, optionsPrecedence, "Uploader"))
            .AppendLine(GetHelpText<AuthorsHelpOptions>(parser, args, optionsPrecedence, "Authors"))
            .AppendLine(GetHelpText<TextsHelpOptions>(parser, args, optionsPrecedence, "Texts"))
            .AppendLine(GetHelpText<EncodingHelpOptions>(parser, args, optionsPrecedence, "Encoding"))
            .AppendLine(GetHelpText<HTMLColorsHelpOptions>(parser, args, optionsPrecedence, "HTML Colors"))
            .AppendLine(GetHelpText<ConfigurationHelpOptions>(parser, args, optionsPrecedence, "Configuration"))
            .AppendLine(GetHelpText<ConvertExamples>(parser, args, optionsPrecedence, "Convert Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<DownloadExamples>(parser, args, optionsPrecedence, "Download Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<UploaderExamples>(parser, args, optionsPrecedence, "Uploader Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<AuthorsExamples>(parser, args, optionsPrecedence, "Authors Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<TextsExamples>(parser, args, optionsPrecedence, "Texts Examples"))
            .AppendLine();
    }

    private static string GetHelpText<T>(Parser parser, string[] args, string[] optionsPrecedence, string heading = null, params string[] descriptionLines)
    {
        return HelpText.AutoBuild(
            parser.ParseArguments<T>(args),
            h =>
            {
                if (descriptionLines.HasAny())
                    h.AddPreOptionsLines(descriptionLines);

                var optionDescriptions = typeof(T).GetInterfaces().SelectMany(it => it.GetCustomAttributes<OptionDescriptionAttribute>().Select(x => x.Description));
                if (optionDescriptions.HasAny())
                    h.AddPreOptionsLines(optionDescriptions);

                h.Heading = (
                    string.IsNullOrEmpty(heading) ?
                    string.Empty :
                    $"{heading}{Environment.NewLine}{new string('-', heading.Length)}"
                );

                h.Copyright = string.Empty;
                h.AdditionalNewLineAfterOption = false;
                h.MaximumDisplayWidth = 120;
                h.AddNewLineBetweenHelpSections = true;
                h.AddDashesToOption = true;
                h.AutoHelp = false;
                h.AutoVersion = false;
                h.OptionComparison = (attr1, attr2) =>
                {
                    int OrderOptions(string longName)
                    {
                        if (attr1.LongName == longName)
                            return -1;
                        if (attr2.LongName == longName)
                            return 1;
                        return 0;
                    }

                    foreach (var option in optionsPrecedence)
                    {
                        int value;
                        if ((value = OrderOptions(option)) != 0)
                            return value;
                    }

                    return attr1.LongName.CompareTo(attr2.LongName);
                };

                return h;
            },
            e => e
        );
    }
}
