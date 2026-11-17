using AgentSkillsLab.Console.Parsing;

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
string? inputPath;
try
{
    inputPath = SelectInputFile(args, dataDirectory);
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
{
    Console.Error.WriteLine($"Could not access input data: {exception.Message}");
    return 1;
}

if (inputPath is null)
{
    return 1;
}

try
{
    Console.WriteLine($"File: {Path.GetFileName(inputPath)}");

    var detection = IcsFileSniffer.Check(inputPath);
    switch (detection.Status)
    {
        case IcsFormatStatus.Recognized:
            Console.WriteLine("ICS file confirmed.");
            break;
        case IcsFormatStatus.NonStandard:
            Console.WriteLine("Non-standard format.");
            Console.ReadLine();
            return 0;
        case IcsFormatStatus.NotRecognized:
            Console.WriteLine("Not recognised.");
            Console.ReadLine();
            return 0;
    }

    var calendar = IcsCalendarParser.Parse(File.ReadAllText(inputPath));

    Console.WriteLine();
    Console.WriteLine("Fields found in VEVENT:");
    foreach (var field in calendar.EventFields)
    {
        Console.WriteLine($"- {field}");
    }

    Console.WriteLine();
    Console.WriteLine($"Events ({calendar.Events.Count} stored VEVENTs):");
    foreach (var calendarEvent in calendar.Events)
    {
        Console.WriteLine($"Date: {calendarEvent.StartDate}; Name: {calendarEvent.Name}");
    }
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or FormatException)
{
    Console.Error.WriteLine($"Could not process the input file: {exception.Message}");
    Console.ReadLine();
    return 1;
}

Console.WriteLine();
Console.WriteLine("Press Enter to exit.");
Console.ReadLine();
return 0;

static string? SelectInputFile(string[] arguments, string dataDirectory)
{
    if (arguments.Length > 1)
    {
        Console.Error.WriteLine("Usage: AgentSkillsLab.Console [file-name]");
        return null;
    }

    if (arguments.Length == 1)
    {
        var suppliedPath = Path.IsPathRooted(arguments[0])
            ? arguments[0]
            : Path.Combine(dataDirectory, arguments[0]);

        if (!File.Exists(suppliedPath))
        {
            Console.Error.WriteLine($"Input file not found: {suppliedPath}");
            return null;
        }

        return suppliedPath;
    }

    if (!Directory.Exists(dataDirectory))
    {
        Console.Error.WriteLine($"Data directory not found: {dataDirectory}");
        return null;
    }

    var files = Directory.GetFiles(dataDirectory).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToArray();
    if (files.Length == 0)
    {
        Console.Error.WriteLine($"No input files found in: {dataDirectory}");
        return null;
    }

    if (files.Length == 1)
    {
        return files[0];
    }

    Console.WriteLine("Select an input file:");
    for (var index = 0; index < files.Length; index++)
    {
        Console.WriteLine($"{index + 1}. {Path.GetFileName(files[index])}");
    }

    Console.Write("File number: ");
    var selection = Console.ReadLine();
    if (int.TryParse(selection, out var selectedIndex) && selectedIndex >= 1 && selectedIndex <= files.Length)
    {
        return files[selectedIndex - 1];
    }

    Console.Error.WriteLine("Invalid file selection.");
    return null;
}
