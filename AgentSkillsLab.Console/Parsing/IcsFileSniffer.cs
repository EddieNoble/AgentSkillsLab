namespace AgentSkillsLab.Console.Parsing;

public enum IcsFormatStatus
{
    Recognized,
    NonStandard,
    NotRecognized,
}

public sealed record IcsFormatCheck(bool HasIcsExtension, bool HasCalendarOpeningLine)
{
    public IcsFormatStatus Status =>
        (HasIcsExtension, HasCalendarOpeningLine) switch
        {
            (true, true) => IcsFormatStatus.Recognized,
            (false, false) => IcsFormatStatus.NotRecognized,
            _ => IcsFormatStatus.NonStandard,
        };
}

public static class IcsFileSniffer
{
    public static IcsFormatCheck Check(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var hasIcsExtension = string.Equals(Path.GetExtension(filePath), ".ics", StringComparison.OrdinalIgnoreCase);

        using var reader = new StreamReader(filePath);
        var firstLine = reader.ReadLine();
        var hasCalendarOpeningLine = string.Equals(
            firstLine?.Trim(),
            "BEGIN:VCALENDAR",
            StringComparison.OrdinalIgnoreCase
        );

        return new IcsFormatCheck(hasIcsExtension, hasCalendarOpeningLine);
    }
}
