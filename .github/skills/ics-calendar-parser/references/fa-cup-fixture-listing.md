# FA Cup Fixture Listing: First-Pass Reference

## Purpose and status

This reference records the decisions used for the first human-reviewed listing
from `AgentSkillsLab.Console/Data/the-fa-cup.ics`. It is a reproducible starting
point, not a claim that the current ICS parser supports every ICS calendar or
that these are the final business rules.

## First-pass output

Produce one line per stored fixture, in chronological start-time order:

```text
YYYY-MM-DD — Fixture summary
```

The first pass produced 60 fixture lines.

## Decisions and assumptions

1. Treat each stored top-level `VEVENT` as one fixture. Do not expand recurrence
   rules, deduplicate events, or filter events by `STATUS`.
2. Use `DTSTART` as the event start. In this input, all 60 parsed starts have the
   form `DTSTART:YYYYMMDDTHHMMSSZ`; `Z` denotes UTC.
3. Sort by the full UTC start instant, not only by its date. Display the UTC date
   as `YYYY-MM-DD`, without a time or timezone conversion. Preserve source order
   when two events have the same start instant.
4. Use `SUMMARY` as the fixture string. It directly gives the matchup (for
   example, `Bristol Rovers v Cambridge United`).
5. Do not append `LOCATION` or use `DESCRIPTION` in the fixture string.
   `DESCRIPTION` repeats the matchup and adds venue and competition details;
   those may be useful in a later, explicitly requested output.
6. Do not normalize team names, infer missing fields, or otherwise enrich the
   fixture string. These choices were made for a concise first pass and can be
   changed by the human.

## Reproducing the extraction

1. Check that the input file exists locally. It is intentionally excluded from
   source control by the root `.gitignore`, because real calendar files may
   contain sensitive details. If it is unavailable, ask the human for the data;
   do not silently substitute or download a different calendar.
2. Confirm that the file is recognized as ICS using the existing
   `IcsFileSniffer`, then parse it with `IcsCalendarParser.Parse`.
3. The parser's `CalendarEvent.StartDate` is the raw `DTSTART` property line,
   not a normalized date value. `CalendarEvent.Name` is the parsed `SUMMARY`.
   Preserve each event's original index for stable tie ordering.
4. For this specific input, parse the value after `DTSTART:` using the exact
   UTC pattern `yyyyMMdd'T'HHmmss'Z'` and invariant culture. Sort by that UTC
   instant, using original index as the tie-breaker.
5. Format the UTC date as `yyyy-MM-dd` and pair it with `Name`. Verify that
   exactly 60 entries were produced.
6. Do not assume the console application's current event display is sorted: it
   displays stored events in source order. The first-pass sorted list was
   produced by ordering the parsed starts separately; no sorting behavior was
   added to the application.
7. If a later calendar contains date-only starts, local/floating times,
   `TZID` parameters, missing starts, or other date forms, stop and clarify the
   required date and timezone interpretation rather than reusing the
   FA Cup-specific timestamp assumption.

## Continuation and handoff

Continue the user's data-analysis exercise one requested increment at a time.
Do not treat this reference as approval to add broader parsing or analytics
features. Make assumptions visible, distinguish observed facts from
interpretation, and show evidence for extracted results.

Before handing work to the next agent or ending a work session, prepare a brief
for the human that lets them resume the work later. Include:

- the input used and whether it was available locally;
- what was actually extracted or tested, including counts and any anomalies;
- decisions the human has confirmed, assumptions still in use, and questions
  still open;
- code or reference files changed, validation performed, and any commit status;
- the next concrete step, without deciding it on the human's behalf.

Keep the brief factual and limited to the work and its technical decisions. Do
not include separate private evaluative observations about the human-agent
collaboration.
