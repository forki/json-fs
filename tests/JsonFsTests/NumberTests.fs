module NumberTests

open Xunit
open FsUnit.Xunit
open JsonFs
open System

[<Fact>]
let ``the numeric string "0" is parsed into a decimal value of 0``() =
    let result = Json.parse "0"

    result |> should equal (Json.Number 0M)

[<Fact>]
let ``the numeric string "01" is parsed into a decimal value of 1``() =
    let result = Json.parse "01"

    result |> should equal (Json.Number 1M)

[<Fact>]
let ``the numeric string "123456789" is parsed into a decimal value of 123456789``() =
    let result = Json.parse "123456789"

    result |> should equal (Json.Number 123456789M)

[<Fact>]
let ``the numeric string "-1" is parsed into a decimal value of -1``() =
    let result = Json.parse "-1"

    result |> should equal (Json.Number -1M)

[<Fact>]
let ``the numeric string "-0" is parsed into a decimal value of 0``() =
    let result = Json.parse "-0"

    result |> should equal (Json.Number 0M)

[<Fact>]
let ``the numeric string "3.14159" is parsed into a decimal value of 3.14159``() =
    let result = Json.parse "3.14159"

    result |> should equal (Json.Number 3.14159M)

[<Fact>]
let ``the numeric string "3." is parsed into a decimal value of 3``() =
    let result = Json.parse "3."

    result |> should equal (Json.Number 3M)

[<Fact>]
let ``the numeric string "1.2345E-02" is parsed into a decimal value of 1.2345E-02``() =
    let result = Json.parse "1.2345E-02"

    result |> should equal (Json.Number 1.2345E-02M)

[<Fact>]
let ``the numeric string "1.2345e+02" is parsed into a decimal value of 1.2345e+02``() =
    let result = Json.parse "1.2345e+02"

    result |> should equal (Json.Number 1.2345e+02M)

[<Fact>]
let ``the numeric string "1.2345e02" is parsed into a decimal value of 1.2345e02``() =
    let result = Json.parse "1.2345e02"

    result |> should equal (Json.Number 1.2345e02M)

[<Fact>]
let ``the numeric string "1.2345e" contains an invalid exponent and an exception is thrown``() =
    (fun() -> Json.parse "1.2345e" |> ignore) |> should throw typeof<UnrecognisedJsonException>

[<Fact>]
let ``the numeric string "1.2345E+" contains an invalid exponent and an exception is thrown``() =
    (fun() -> Json.parse "1.2345E+" |> ignore) |> should throw typeof<UnrecognisedJsonException>