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

[<Fact>]
let ``a number is formatted correctly``() =
    let result = Json.format (Number 123M)

    result |> should equal "123"

[<Fact>]
let ``a string is formatted correctly``() =
    let result = Json.format (String "hello world")

    result |> should equal "\"hello world\""

[<Fact>]
let ``an array is formatted correctly``() =
    let result = Json.format (Array [Number 1M; Number 2M; Number 3M])

    result |> should equal "[1,2,3]"

[<Fact>]
let ``an object is formatted correctly``() =
    let expected = [("name", String "john doe")] |> Map.ofList
    let result = Json.format (Object expected)

    result |> should equal "{\"name\":\"john doe\"}"
