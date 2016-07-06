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

[<Fact>]
let ``a string containing unicode sequence '\u0aE0' (with mixed case) is parsed as character 'ૠ'``()=
    let result = Json.parse "\"\\u0aE0\""

    result |> should equal (Json.String "ૠ")

[<Fact>]
let ``a string containing a single space is parsed without escaping``()=
    let result = Json.parse "\" \""

    result |> should equal (Json.String " ")

[<Fact>]
let ``a string containing a single exclamation mark is parsed without escaping``()=
    let result = Json.parse "\"!\""

    result |> should equal (Json.String "!")

[<Fact>]
let ``a string containing a full range of supported ASCII characters is parsed without escaping``()=
    let result = Json.parse "\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\""

    result |> should equal (Json.String "#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~")

[<Fact>]
let ``a string containing characters upto the end of the basic multilingual plane are parsed without escaping``()=
    let result = Json.parse "\"གྷᡵヶ⢇𐿿\""

    result |> should equal (Json.String "གྷᡵヶ⢇𐿿")

[<Fact>]
let ``a string "abcdefg" is parsed when trailed by whitespace``()=
    let result = Json.parse "\"abcdefg\"    "

    result |> should equal (Json.String "abcdefg")

