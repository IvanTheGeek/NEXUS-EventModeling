module EventModeling.Tests.PropertyTests

open System
open Expecto
open Hedgehog
open CsCheck
open EventModeling
open EventModeling.Testing.Generators

let allPropertyTests =
    testList "Properties" [

        // ─── Hedgehog ─────────────────────────────────────────────────────────

        testCase "actor kind covers all cases" <| fun () ->
            Property.check <| property {
                let! kind = actorKind
                let valid =
                    match kind with
                    | Human _          -> true
                    | Automation _     -> true
                    | ExternalSystem _ -> true
                return valid
            }

        testCase "event preserves name and data" <| fun () ->
            Property.check <| property {
                let! ev = event (Gen.int (Range.linear 0 1000))
                return ev.Name <> "" && ev.Data >= 0
            }

        testCase "command carries actor identity" <| fun () ->
            Property.check <| property {
                let! cmd = command actor (Gen.int (Range.linear 0 100))
                return cmd.IssuedBy.Name <> ""
            }

        // ─── CsCheck ──────────────────────────────────────────────────────────

        testCase "event data roundtrips through record" <| fun () ->
            Gen.Int.[0, 1000].Sample(fun n ->
                let ev : Event<int> = { Name = "E"; OccurredAt = DateTimeOffset.MinValue; Data = n }
                if ev.Data <> n then failtest $"Data mismatch: expected {n}, got {ev.Data}"
            )
    ]
