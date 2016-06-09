module NumberTests

open Xunit
open FsUnit.Xunit
open JsonFs
open System

[<Fact>]
let ``the string "0" is parsed into a decimal value of 0``() =
    let result = Json.parse "0"

    result |> should equal (Json.Number 0M)

[<Fact>]
let ``the string "01" is parsed into a decimal value of 0``() =
    let result = Json.parse "01"

    result |> should equal (Json.Number 0M)

[<Fact>]
let ``the string "123456789" is parsed into a decimal value of 123456789``() =
    let result = Json.parse "123456789"

    result |> should equal (Json.Number 123456789M)