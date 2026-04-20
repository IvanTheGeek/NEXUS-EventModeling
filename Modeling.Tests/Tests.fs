module EventModeling.Tests.Tests

open System
open Expecto
open EventModeling
open EventModeling.Testing.Assert
open EventModeling.Testing.GroupingRunner

// ─── Abstract framework types ─────────────────────────────────────────────────
// No domain meaning. These exist only to exercise the framework plumbing.

type FxState   = { N: int }
type FxCommand = { N: int }
type FxEvent   = { N: int }
type FxCrit    = { Min: int }
type FxView    = { Items: int list }

// ─── Actors ───────────────────────────────────────────────────────────────────

let system1 = { Name = "System"; Kind = Automation "system"; Swimlane = None }
let user1   = { Name = "User";   Kind = Human "user";        Swimlane = None }

// ─── CommandSlice ─────────────────────────────────────────────────────────────

let commandSlice : CommandSlice<Actor, FxState, FxCommand, FxEvent> =
    let handler (_given: Event<FxState> list) (cmd: Command<FxCommand>) =
        Ok [ { Name = "FxEvent"; OccurredAt = DateTimeOffset.UtcNow; Data = { N = cmd.Data.N } } ]

    { Actor   = system1
      Command = { Name = "FxCommand"; IssuedBy = system1; Data = { N = 1 } }
      Handler = handler
      GWT =
        { Given = []
          When  = { Name = "FxCommand"; IssuedBy = system1; Data = { N = 1 } }
          Then  = [ { Name = "FxEvent"; OccurredAt = DateTimeOffset.MinValue; Data = { N = 1 } } ] } }

// ─── ViewSlice ────────────────────────────────────────────────────────────────

let viewSlice : ViewSlice<FxEvent, FxCrit, FxView, Actor> =
    let handler (events: Event<FxEvent> list) (crit: FxCrit) =
        let items = events |> List.map (fun e -> e.Data.N) |> List.filter (fun n -> n >= crit.Min)
        { Name = "FxView"; Data = { Items = items } }

    let ev n = { Name = "FxEvent"; OccurredAt = DateTimeOffset.MinValue; Data = { N = n } }

    { Events  = []
      View    = { Name = "FxView"; Data = { Items = [] } }
      Handler = handler
      GWT =
        { Given = [ ev 1; ev 2; ev 3 ]
          When  = { Min = 2 }
          Then  = { Name = "FxView"; Data = { Items = [ 2; 3 ] } } }
      Actor   = user1 }

// ─── Registry and Grouping ────────────────────────────────────────────────────

let cmdRef  = CommandRef("FxCommand", system1)
let viewRef = ViewRef("FxView", user1)

let registry : SliceRegistry =
    [ cmdRef,  CommandEntry (commandSliceToTest "command handler produces event" commandSlice)
      viewRef, ViewEntry    (viewSliceToTest    "view handler filters by min"   viewSlice) ]
    |> Map.ofList

let frameworkGrouping =
    Area("EventModeling.Framework",
        [ Workflow("CommandSlice", [ Flow("GWT", [ cmdRef  ]) ])
          Workflow("ViewSlice",    [ Flow("GWT", [ viewRef ]) ]) ])

let allTests = groupingToTest registry frameworkGrouping
