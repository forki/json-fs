module FormattingTests

open Xunit
open FsUnit.Xunit
open JsonFs

[<Fact>]
let ``the literal null is formatted correctly``() =
    let result = Json.format (Null ())

    result |> should equal "null"

[<Fact>]
let ``the literal true is formatted correctly``() =
    let result = Json.format (Bool true)

    result |> should equal "true"

[<Fact>]
let ``the literal false is formatted correctly``() =
    let result = Json.format (Bool false)

    result |> should equal "false"