module EventModeling.Testing.Generators

open System
open Hedgehog
open EventModeling

let actorKind : Gen<ActorKind> =
    Gen.choice [
        Gen.string (Range.linear 1 20) Gen.alphaNum |> Gen.map Human
        Gen.string (Range.linear 1 20) Gen.alphaNum |> Gen.map Automation
        Gen.string (Range.linear 1 20) Gen.alphaNum |> Gen.map ExternalSystem
    ]

let actor : Gen<Actor> =
    gen {
        let! name     = Gen.string (Range.linear 1 20) Gen.alphaNum
        let! kind     = actorKind
        let! swimlane = Gen.choice [ Gen.constant None
                                     Gen.string (Range.linear 1 10) Gen.alphaNum |> Gen.map Some ]
        return { Name = name; Kind = kind; Swimlane = swimlane }
    }

let event (dataGen: Gen<'T>) : Gen<Event<'T>> =
    gen {
        let! name = Gen.string (Range.linear 1 20) Gen.alphaNum
        let! data = dataGen
        return { Name = name; OccurredAt = DateTimeOffset.MinValue; Data = data }
    }

let command (actorGen: Gen<Actor>) (dataGen: Gen<'T>) : Gen<Command<'T>> =
    gen {
        let! name     = Gen.string (Range.linear 1 20) Gen.alphaNum
        let! issuedBy = actorGen
        let! data     = dataGen
        return { Name = name; IssuedBy = issuedBy; Data = data }
    }

let view (dataGen: Gen<'T>) : Gen<View<'T>> =
    gen {
        let! name = Gen.string (Range.linear 1 20) Gen.alphaNum
        let! data = dataGen
        return { Name = name; Data = data }
    }
