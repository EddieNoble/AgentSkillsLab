using AgentSkillsLab.Console.Parsing;

namespace AgentSkillsLab.Tests;

public class IcsFileSnifferTests
{
    [Theory]
    [InlineData("calendar.ics", "BEGIN:VCALENDAR", IcsFormatStatus.Recognized)]
    [InlineData("CALENDAR.ICS", "begin:vcalendar", IcsFormatStatus.Recognized)]
    [InlineData("calendar.txt", "BEGIN:VCALENDAR", IcsFormatStatus.NonStandard)]
    [InlineData("calendar.ics", "not a calendar", IcsFormatStatus.NonStandard)]
    [InlineData("calendar.txt", "not a calendar", IcsFormatStatus.NotRecognized)]
    public void Check_CombinesExtensionAndOpeningLine(
        string fileName,
        string firstLine,
        IcsFormatStatus expectedStatus
    )
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var path = Path.Combine(directory, fileName);
            File.WriteAllText(path, $"{firstLine}{Environment.NewLine}content");

            var result = IcsFileSniffer.Check(path);

            Assert.Equal(expectedStatus, result.Status);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
