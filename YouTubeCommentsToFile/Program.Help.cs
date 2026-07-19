using CommandLine;
using CommandLine.Text;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static StringBuilder GetHelp(Parser parser)
    {
        string[] optionsPrecedence = OptionsHelper.GetOptionsPrecedence();

        return new StringBuilder()
            .AppendLine(GetVersion())
            .AppendLine()
            .AppendLine(GetHelpText<ConvertHelpOptions>(parser, optionsPrecedence, "Convert"))
            .AppendLine(GetHelpText<DownloadHelpOptions>(parser, optionsPrecedence, "Download"))
            .AppendLine(GetHelpText<SharedHelpOptions>(parser, optionsPrecedence, "Shared Options"))
            .AppendLine(GetHelpText<UploaderHelpOptions>(parser, optionsPrecedence, "Uploader"))
            .AppendLine(GetHelpText<AuthorsHelpOptions>(parser, optionsPrecedence, "Authors"))
            .AppendLine(GetHelpText<TextsHelpOptions>(parser, optionsPrecedence, "Texts"))
            .AppendLine(GetHelpText<EncodingHelpOptions>(parser, optionsPrecedence, "Encoding"))
            .AppendLine(GetHelpText<HTMLColorsHelpOptions>(parser, optionsPrecedence, "HTML Colors"))
            .AppendLine(GetHelpText<ConfigurationHelpOptions>(parser, optionsPrecedence, "Configuration"))
            .AppendLine(GetHelpText<VerbosityHelpOptions>(parser, optionsPrecedence, "Verbosity"))
            .AppendLine(GetHelpText<GeneralHelpOptions>(parser, optionsPrecedence, "General"))
            .AppendLine(GetHelpText<ConvertExamples>(parser, optionsPrecedence, "Convert Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<DownloadExamples>(parser, optionsPrecedence, "Download Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<UploaderExamples>(parser, optionsPrecedence, "Uploader Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<AuthorsExamples>(parser, optionsPrecedence, "Authors Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<TextsExamples>(parser, optionsPrecedence, "Texts Examples"))
            .AppendLine();
    }

    private static string GetHelpText<T>(Parser parser, string[] optionsPrecedence, string heading = null, params string[] descriptionLines)
    {
        return HelpText.AutoBuild(
            parser.ParseArguments<T>(["--help"]),
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
                    foreach (var option in optionsPrecedence)
                    {
                        int value;
                        if ((value = OrderOptions(option)) != 0)
                            return value;
                    }

                    int OrderOptions(string longName)
                    {
                        if (attr1.LongName == longName)
                            return -1;
                        if (attr2.LongName == longName)
                            return 1;
                        return 0;
                    }

                    return attr1.LongName.CompareTo(attr2.LongName);
                };

                return h;
            },
            e => e
        );
    }
}
