# NEXUS-EventModeling

> Sync contract: when `EVENT_MODELING.md` or `FSHARP.md` is updated, update the corresponding condensed section below.

## Purpose

This is the NEXUS ecosystem-wide base library for Event Modeling in F#.

Source of truth for:
- The `EventModeling` core library (Actor, Command, Event, View, Slice, Path, Grouping types)
- The `EventModeling.Testing` library — GWT adapters, grouping runner, Hedgehog generators, full testing stack

## Stack

- **Language**: F# on .NET 10.0
- **Test framework**: Expecto
- **Solution**: `EventModeling.slnx`

## Project Layout

```
EventModeling/           Core library — domain types only
EventModeling.Testing/   Test utility library — GWT adapters, grouping runner, Hedgehog generators
EventModeling.Tests/     Framework validation tests — abstract types, no domain
```

## Event Modeling

> Condensed from `EVENT_MODELING.md`. Update that file AND this section together.

- **Actor**: source of decisions and actions — `Human of role`, `Automation of name`, `ExternalSystem of name`
- **Command**: intent expressed by an Actor; handled by a hidden pure CommandHandler; blue box
- **Event**: immutable fact, past tense, never changes; the backbone and the only coupling point; orange box
- **View**: shaped data produced by a hidden EventHandler for an Actor to consume; green box
- **CommandSlice**: `Actor → Command → Event(s)` — data flows top-down; state change
- **ViewSlice**: `Event(s) → View → Actor` — data flows bottom-up; state view
- **Cadence**: CommandSlice → ViewSlice → CommandSlice → ViewSlice (sine wave)
- **GWT**: Given/When/Then — simultaneously the business spec and the test suite; example data IS the tests
  - CommandSlice GWT: Given = prior events (state), When = command, Then = resulting events
  - ViewSlice GWT: Given = events, When = filter/sort/transform criteria, Then = shaped view
- **Flow**: smallest grouping — short sequence of slices for one focused interaction
- **Workflow**: series of Flows or nested Workflows for a larger process; nestable, no depth limit
- **Area**: top-level grouping of related Workflows
- Groupings are header rows above the grid — purely organizational, no logic
- **Slice immutability**: finalized slices are replaced, never modified; no code shared between slices
- `SliceRef` decouples the Grouping/Path hierarchy from the internal generic types of each slice
- **Conway's Law**: event rows can mirror team/system/bounded-context boundaries
- Handlers are hidden; GWT is visible — GWT rows are the specification handlers implement
- Handlers are pure functions — no side effects, no I/O, no state

## F#

> Condensed from `FSHARP.md`. Update that file AND this section together.

**Scott Wlaschin / Domain Modeling Made Functional**
- Make illegal states unrepresentable — encode invariants in types, not prose
- Use discriminated unions instead of strings/bools/ints for domain concepts
- Total functions: if a function can fail, encode it in the return type (`Result`, `option`) — never throw for domain failures
- Parse, don't validate: validate at the boundary; inside the domain everything is already valid by construction
- Use records for all data types — immutable by default
- Compose functions; F# has no data inheritance

**Compiler rules**
- Compile order in `.fsproj` is mandatory — a type used before it is defined is a compile error
- `namespace` for type-only files; `module` when the file contains `let` bindings/functions
- Recursive functions require `rec`; mutually recursive types require `and`
- Pattern matching is exhaustive — adding a DU case will flag every incomplete match
- `open` is file-scoped — every file that needs a namespace must open it explicitly

**Types and time**
- Always `DateTimeOffset` UTC — never `DateTime`; `OccurredAt` is always UTC; timezone conversion is a view concern
- Use `DateTimeOffset.MinValue` as sentinel in GWT example data (runtime timestamp is not part of the spec)
- Test adapters strip `OccurredAt` before comparing events — only `Name` and `Data` are asserted
- `Result<'T, string>` for CommandHandler failures; the string is a business rule violation message
- `Map` (F# immutable map) is the correct type for `SliceRegistry`
- Single-case DUs wrap primitives to prevent mixing incompatible IDs (e.g. `CustomerId of int`)

**Expecto**
- `testCase "label" <| fun () -> ...` — `<|` avoids nested parens, idiomatic
- `testList "name" [...]` — groups tests; mirrors the Grouping hierarchy
- `runTestsWithCLIArgs [] args allTests` — standard entry point in `Program.fs`

**Hedgehog (property-based)**
- `property { let! x = gen; return bool }` — property CE; `false` triggers shrinking and counterexample
- `Property.check prop` — runs property inside a `testCase`; throws on counterexample
- `Gen.int (Range.linear min max)`, `Gen.string (Range.linear min max) Gen.alphaNum` — common generators
- Generators for core EventModeling types live in `EventModeling.Testing.Generators`

**CsCheck (property-based complement)**
- Use where Hedgehog falls short: shrinking complex types, regression file replay, specific .NET type generators
- `Gen.Int.[min, max].Sample(fun n -> if bad then failtest "...")` — basic F# usage
