module StringTests

open Xunit
open FsUnit.Xunit
open JsonFs

[<Fact>]
let ``a string containing a '"' (quotation mark) is escaped as unicode \u0022``() =
    let result = Json.parse "\"\\\"\""

    result |> should equal (Json.String "\u0022")

[<Fact>]
let ``a string containing a '\' (reverse solidus) is escaped as unicode \u005c``() =
    let result = Json.parse "\"\\\\\""

    result |> should equal (Json.String "\u005c")

[<Fact>]
let ``a string containing a '/' (solidus) is escaped as unicode \u002f``() =
    let result = Json.parse "\"\\/\""

    result |> should equal (Json.String "\u002f")

[<Fact>]
let ``a string containing a '\b' (backspace) is escaped as unicode \u0008``() =
    let result = Json.parse "\"\\b\""

    result |> should equal (Json.String "\u0008")

[<Fact>]
let ``a string containing a '\f' (form feed) is escaped as unicode \u000c``() =
    let result = Json.parse "\"\\f\""

    result |> should equal (Json.String "\u000c")

[<Fact>]
let ``a string containing a '\n' (line feed) is escaped as unicode \u000a``() =
    let result = Json.parse "\"\\n\""

    result |> should equal (Json.String "\u000a")

[<Fact>]
let ``a string containing a '\r' (carriage return) is escaped as unicode \u000d``() =
    let result = Json.parse "\"\\r\""

    result |> should equal (Json.String "\u000d")

[<Fact>]
let ``a string containing a '\t' (tab) is escaped as unicode \u0009``() =
    let result = Json.parse "\"\\t\""

    result |> should equal (Json.String "\u0009")