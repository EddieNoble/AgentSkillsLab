using System.Text;

namespace AgentSkillsLab.Console.Parsing;

public sealed record CalendarEvent(string StartDate, string Name);

public sealed record ParsedCalendar(IReadOnlyCollection<string> EventFields, IReadOnlyList<CalendarEvent> Events);

public static class IcsCalendarParser
{
    public static ParsedCalendar Parse(string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        var fields = new SortedSet<string>(StringComparer.Ordinal);
        var events = new List<CalendarEvent>();
        EventBuilder? currentEvent = null;
        var nestedComponentDepth = 0;

        foreach (var line in UnfoldLines(contents))
        {
            if (currentEvent is null)
            {
                if (line.Equals("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    currentEvent = new EventBuilder();
                    nestedComponentDepth = 0;
                }

                continue;
            }

            if (line.Equals("END:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                if (nestedComponentDepth != 0)
                {
                    throw new FormatException("VEVENT ended before a nested component was closed.");
                }

                events.Add(currentEvent.Build());
                currentEvent = null;
                continue;
            }

            if (line.StartsWith("BEGIN:", StringComparison.OrdinalIgnoreCase))
            {
                if (line.Equals("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException("Nested VEVENT components are not supported.");
                }

                nestedComponentDepth++;
                continue;
            }

            if (line.StartsWith("END:", StringComparison.OrdinalIgnoreCase))
            {
                if (nestedComponentDepth > 0)
                {
                    nestedComponentDepth--;
                }

                continue;
            }

            if (nestedComponentDepth > 0 || string.IsNullOrEmpty(line))
            {
                continue;
            }

            var property = ParseProperty(line);
            fields.Add(property.Name);
            currentEvent.Add(property.Name, property.Value, line);
        }

        if (currentEvent is not null)
        {
            throw new FormatException("Calendar contains an unterminated VEVENT component.");
        }

        return new ParsedCalendar(fields.ToArray(), events);
    }

    private static IEnumerable<string> UnfoldLines(string contents)
    {
        using var reader = new StringReader(contents);
        string? pendingLine = null;

        while (reader.ReadLine() is { } line)
        {
            if (line.StartsWith(' ') || line.StartsWith('\t'))
            {
                if (pendingLine is null)
                {
                    throw new FormatException("Calendar contains a folded line without a preceding line.");
                }

                pendingLine += line[1..];
                continue;
            }

            if (pendingLine is not null)
            {
                yield return pendingLine;
            }

            pendingLine = line;
        }

        if (pendingLine is not null)
        {
            yield return pendingLine;
        }
    }

    private static ParsedProperty ParseProperty(string line)
    {
        var colonIndex = line.IndexOf(':');
        if (colonIndex <= 0)
        {
            throw new FormatException("Calendar contains a malformed property inside VEVENT.");
        }

        var propertyName = line[..colonIndex].Split(';', 2)[0];
        var groupSeparator = propertyName.LastIndexOf('.');
        if (groupSeparator >= 0)
        {
            propertyName = propertyName[(groupSeparator + 1)..];
        }

        if (propertyName.Length == 0)
        {
            throw new FormatException("Calendar contains a property with no name inside VEVENT.");
        }

        return new ParsedProperty(propertyName.ToUpperInvariant(), line[(colonIndex + 1)..]);
    }

    private sealed class EventBuilder
    {
        private string? _startDate;
        private string? _name;

        public void Add(string propertyName, string value, string rawLine)
        {
            if (propertyName.Equals("DTSTART", StringComparison.OrdinalIgnoreCase) && _startDate is null)
            {
                _startDate = rawLine;
            }

            if (propertyName.Equals("SUMMARY", StringComparison.OrdinalIgnoreCase) && _name is null)
            {
                _name = UnescapeText(value);
            }
        }

        public CalendarEvent Build()
        {
            var startDate = string.IsNullOrWhiteSpace(_startDate) ? "No Date" : _startDate;
            var name = string.IsNullOrWhiteSpace(_name) ? "No Name" : _name;
            return new CalendarEvent(startDate, name);
        }

        private static string UnescapeText(string value)
        {
            var result = new StringBuilder(value.Length);
            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] != '\\' || index + 1 >= value.Length)
                {
                    result.Append(value[index]);
                    continue;
                }

                index++;
                result.Append(
                    value[index] switch
                    {
                        'n' or 'N' => ' ',
                        '\\' => '\\',
                        ',' => ',',
                        ';' => ';',
                        _ => value[index],
                    }
                );
            }

            return result.ToString();
        }
    }

    private sealed record ParsedProperty(string Name, string Value);
}
