module LiteralValueTests

open Xunit
open FsUnit.Xunit
open JsonFs
open System

[<Fact>]
let ``the literal "true" is correctly parsed into a boolean``() =
    let result = Json.parse "true"

    result |> should equal (Json.Bool true)

[<Fact>]
let ``the literal "false" is correctly parsed into a boolean``() =
    let result = Json.parse "false"

    result |> should equal (Json.Bool false)

[<Fact>]
let ``the literal "null" is correctly parsed into a unit``() =
    let result = Json.parse "null"

    result |> should equal (Json.Null ())

[<Fact>]
let ``the literal "true" must in lowercase to be parsed otherwise an exception is thrown``() =
    (fun() -> Json.parse "True" |> ignore) |> should throw typeof<UnrecognisedJsonException>

[<Fact>]
let ``the literal "false" must in lowercase to be parsed otherwise an exception is thrown``() =
    (fun() -> Json.parse "False" |> ignore) |> should throw typeof<UnrecognisedJsonException>

[<Fact>]
let ``the literal "null" must be in lowercase to be parsed otherwise an exception is thrown``() =
    (fun() -> Json.parse "Null" |> ignore) |> should throw typeof<UnrecognisedJsonException>