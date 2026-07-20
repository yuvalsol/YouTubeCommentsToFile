using CommandLine;
using CommandLine.Text;

namespace YouTubeCommentsToFile;

internal static partial class Program
{
    private static StringBuilder GetHelp(Parser parser)
    {
        return new StringBuilder()
            .AppendLine(GetVersion())
            .AppendLine()
            .AppendLine(GetHelpText<ConvertHelpOptions>(parser, "Convert"))
            .AppendLine(GetHelpText<DownloadHelpOptions>(parser, "Download"))
            .AppendLine(GetHelpText<SharedHelpOptions>(parser, "Shared Options"))
            .AppendLine(GetHelpText<UploaderHelpOptions>(parser, "Uploader"))
            .AppendLine(GetHelpText<AuthorsHelpOptions>(parser, "Authors"))
            .AppendLine(GetHelpText<TextsHelpOptions>(parser, "Texts"))
            .AppendLine(GetHelpText<EncodingHelpOptions>(parser, "Encoding"))
            .AppendLine(GetHelpText<HTMLColorsHelpOptions>(parser, "HTML Colors"))
            .AppendLine(GetHelpText<ConfigurationHelpOptions>(parser, "Configuration"))
            .AppendLine(GetHelpText<VerbosityHelpOptions>(parser, "Verbosity"))
            .AppendLine(GetHelpText<GeneralHelpOptions>(parser, "General"))
            .AppendLine(GetHelpText<ConvertExamples>(parser, "Convert Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<DownloadExamples>(parser, "Download Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<UploaderExamples>(parser, "Uploader Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<AuthorsExamples>(parser, "Authors Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<TextsExamples>(parser, "Texts Examples"))
            .AppendLine();
    }

    private static string GetHelpText<T>(Parser parser, string heading = null, params string[] descriptionLines)
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
                    int index1 = OptionsHelper.OptionsPrecedence.FirstOrDefault(x => x.LongName == attr1.LongName, OptionsHelper.OptionPrecedence.Default).Index;
                    int index2 = OptionsHelper.OptionsPrecedence.FirstOrDefault(x => x.LongName == attr2.LongName, OptionsHelper.OptionPrecedence.Default).Index;
                    return (index1 != index2 ? index1.CompareTo(index2) : attr1.LongName.CompareTo(attr2.LongName));
                };

                return h;
            },
            e => e
        );
    }
}
