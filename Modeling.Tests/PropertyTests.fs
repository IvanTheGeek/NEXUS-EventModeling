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
        // property { let! x = gen; return bool } uses BindReturn → Property<bool>
        // use Property.checkBool, not Property.check

        testCase "actor kind covers all cases" <| fun () ->
            Property.checkBool <| property {
                let! kind = actorKind
                return
                    match kind with
                    | Human _          -> true
                    | Automation _     -> true
                    | ExternalSystem _ -> true
            }

        testCase "event preserves name and data" <| fun () ->
            Property.checkBool <| property {
                let! ev = event (Gen.int32 (Range.linear 0 1000))
                return ev.Name <> "" && ev.Data >= 0
            }

        testCase "command carries actor identity" <| fun () ->
            Property.checkBool <| property {
                let! cmd = command actor (Gen.int32 (Range.linear 0 100))
                return cmd.IssuedBy.Name <> ""
            }

        // ─── CsCheck ──────────────────────────────────────────────────────────
        // Sample is a static method on Check, not on Gen<T>

        testCase "event data roundtrips through record" <| fun () ->
            Check.Sample(Gen.Int.[0, 1000], fun n ->
                let ev : Event<int> = { Name = "E"; OccurredAt = DateTimeOffset.MinValue; Data = n }
                if ev.Data <> n then failtest $"Data mismatch: expected {n}, got {ev.Data}"
            )
    ]
