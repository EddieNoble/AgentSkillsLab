using AgentSkillsLab.Console.Parsing;

namespace AgentSkillsLab.Tests;

public class IcsCalendarParserTests
{
    [Fact]
    public void Parse_ReturnsUniqueVEventFieldsAndOneRowPerStoredEvent()
    {
        const string calendar = """
            BEGIN:VCALENDAR
            BEGIN:VTIMEZONE
            TZID:Example/Zone
            END:VTIMEZONE
            BEGIN:VEVENT
            DTSTART;TZID=Example/Zone:20260101T090000
            SUMMARY:Planning\, review
            UID:first
            END:VEVENT
            BEGIN:VEVENT
            DTSTART:20260102
            UID:second
            END:VEVENT
            BEGIN:VEVENT
            UID:third
            END:VEVENT
            END:VCALENDAR
            """;

        var parsed = IcsCalendarParser.Parse(calendar);

        Assert.Equal(new[] { "DTSTART", "SUMMARY", "UID" }, parsed.EventFields);
        Assert.Equal(3, parsed.Events.Count);
        Assert.Equal("DTSTART;TZID=Example/Zone:20260101T090000", parsed.Events[0].StartDate);
        Assert.Equal("Planning, review", parsed.Events[0].Name);
        Assert.Equal("DTSTART:20260102", parsed.Events[1].StartDate);
        Assert.Equal("No Name", parsed.Events[1].Name);
        Assert.Equal("No Date", parsed.Events[2].StartDate);
        Assert.Equal("No Name", parsed.Events[2].Name);
    }

    [Fact]
    public void Parse_IgnoresNestedComponentFieldsAndUnfoldsPropertyLines()
    {
        const string calendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART:20260101
            SUMMARY:Planning eve
             nt
            BEGIN:VALARM
            ACTION:DISPLAY
            TRIGGER:-PT15M
            END:VALARM
            END:VEVENT
            END:VCALENDAR
            """;

        var parsed = IcsCalendarParser.Parse(calendar);

        Assert.Equal(new[] { "DTSTART", "SUMMARY" }, parsed.EventFields);
        Assert.Single(parsed.Events);
        Assert.Equal("Planning event", parsed.Events[0].Name);
    }

    [Fact]
    public void Parse_ThrowsWhenAnEventIsNotClosed()
    {
        const string calendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART:20260101
            """;

        Assert.Throws<FormatException>(() => IcsCalendarParser.Parse(calendar));
    }
}
