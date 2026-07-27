using CommandLine.Text;

namespace YouTubeCommentsToFile;

internal partial class Program
{
    private StringBuilder GetHelp()
    {
        return new StringBuilder()
            .AppendLine(GetVersion())
            .AppendLine()
            .AppendLine(GetHelpText<ConvertHelpOptions>("Convert"))
            .AppendLine(GetHelpText<DownloadHelpOptions>("Download"))
            .AppendLine(GetHelpText<SharedHelpOptions>("Shared Options"))
            .AppendLine(GetHelpText<UploaderHelpOptions>("Uploader"))
            .AppendLine(GetHelpText<AuthorsHelpOptions>("Authors"))
            .AppendLine(GetHelpText<TextsHelpOptions>("Texts"))
            .AppendLine(GetHelpText<EncodingHelpOptions>("Encoding"))
            .AppendLine(GetHelpText<HTMLColorsHelpOptions>("HTML Colors"))
            .AppendLine(GetHelpText<ConfigurationHelpOptions>("Configuration"))
            .AppendLine(GetHelpText<VerbosityHelpOptions>("Verbosity"))
            .AppendLine(GetHelpText<GeneralHelpOptions>("General"))
#if DEBUG
            .AppendLine(GetHelpText<DebugHelpOptions>("Debug"))
#endif
            .AppendLine(GetHelpText<ConvertExamples>("Convert Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<DownloadExamples>("Download Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<UploaderExamples>("Uploader Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<AuthorsExamples>("Authors Examples"))
            .AppendLine()
            .AppendLine(GetHelpText<TextsExamples>("Texts Examples"))
            .AppendLine();
    }

    private string GetHelpText<T>(string heading = null)
    {
        return HelpText.AutoBuild(
            parser.ParseArguments<T>(["--help"]),
            h =>
            {
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
