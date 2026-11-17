---
name: ics-calendar-parser
description: Help design, implement, test, or refine ICS calendar parsing in the AgentSkillsLab .NET console app. Use when working on calendar-file input, parsing behavior, event data, or validation.
---

# ICS Calendar Parser

## Goal

Guide incremental development of ICS calendar parsing in
`AgentSkillsLab.Console`. Treat this as a learning example: explain meaningful
trade-offs and keep the implementation small enough to compare with the patterns
used in the MAB codebase.

## Workflow

1. Read the console project and this skill's supporting resources before proposing
   changes.
2. Confirm the requested behavior and user-configurable options before choosing a
   parser library, data model, or interpretation of ICS edge cases.
3. Use `AgentSkillsLab.Console/Data` as the default location for input data. If the
   user has not identified a file and multiple candidates are present, ask which file
   to use rather than silently choosing one.
4. Keep file-format detection, parsing, and console input/output separate so each can
   be tested and extended independently.
5. Add focused tests for the requested behavior, including malformed or incomplete
   input where relevant.
6. Run the focused tests and build the solution. Report commands and any checks that
   could not be run.

Do not assume that a minimal parser is fully RFC 5545-compliant. Call out unsupported
features and clarify requirements before silently ignoring calendar properties,
recurrence rules, time zones, or encoding details.

## Current increment: identify and summarize stored ICS events

Given an input file:

1. Print the file name to the console.
2. Check both whether the file name has an `.ics` suffix (case-insensitive) and
   whether its opening content line is `BEGIN:VCALENDAR`.
3. Report the classification:
   - Both checks pass: confirm that it is an ICS file.
   - Exactly one check passes: report `Non-standard format`.
   - Neither check passes: report `Not recognised`.
4. If the file cannot be found or read, report that error distinctly; do not classify
   an unreadable file as an unrecognised format.
5. Continue only when both ICS checks pass. List the unique properties found directly
   inside `VEVENT` components; do not include calendar or time-zone component fields.
6. Print one row for every stored `VEVENT`, showing its `DTSTART` property as stored
   (including parameters such as `TZID`) and its `SUMMARY`. Use `No Name` when the
   summary is absent or blank, and `No Date` if there is no start property.
7. Do not expand `RRULE` recurrence definitions. This output represents the stored
   `VEVENT` components, including any explicit recurrence overrides.

Keep format detection, ICS parsing, and console display separate. The current ICS
parser is intentionally limited to the requested output; do not describe it as fully
RFC 5545-compliant. The longer-term direction is to support multiple format sniffers
(for example ICS, JSON, and XML), followed by selectable parsers and output/analysis
options such as event counts by month or invitees by date. Preserve that
composability, but do not build a generic plugin system, UI, or analytics framework
until a concrete next requirement calls for it.
