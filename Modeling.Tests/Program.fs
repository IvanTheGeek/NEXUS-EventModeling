module EventModeling.Tests.Program

open Expecto
open EventModeling.Tests.Tests
open EventModeling.Tests.PropertyTests

[<EntryPoint>]
let main args =
    let all = testList "EventModeling" [ allTests; allPropertyTests ]
    runTestsWithCLIArgs [] args all
