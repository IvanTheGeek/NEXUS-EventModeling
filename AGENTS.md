# NEXUS-EventModeling

> Workspace instructions and references: `../AGENTS.md`, `../FSHARP.md`, `../WORKFLOW.md`
> Library sync contract: when `EVENT_MODELING.md` changes, update `## Event Modeling` in workspace `AGENTS.md`.

## Purpose

This is the NEXUS ecosystem-wide base library for Event Modeling in F#.

Source of truth for:
- The `EventModeling` core library (Actor, Command, Event, View, Slice, Path, Grouping types)
- The `EventModeling.Testing` library — GWT adapters, grouping runner, Hedgehog generators, full testing stack

## Stack

- **Language**: F# on .NET 10.0
- **Test framework**: Expecto + Hedgehog + CsCheck
- **Solution**: `EventModeling.slnx`

## Project Layout

```
EventModeling/           Core library — domain types only
EventModeling.Testing/   Test utility library — GWT adapters, grouping runner, Hedgehog generators
EventModeling.Tests/     Framework validation tests — abstract types, no domain
```

## Library Conventions

These apply specifically to this library's implementation:

- `SliceRef` decouples the Grouping/Path hierarchy from the internal generic types of each slice
- `Map` (F# immutable map) is the correct type for `SliceRegistry`
- Use `DateTimeOffset.MinValue` as sentinel in GWT example data — runtime timestamp is not part of the spec
- Test adapters strip `OccurredAt` before comparing events — only `Name` and `Data` are asserted
- Generators for core EventModeling types live in `EventModeling.Testing.Generators`
