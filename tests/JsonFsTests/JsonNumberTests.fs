module JsonNumberTests

open Xunit
open FsUnit.Xunit
open JsonCs
open JsonStreamFactory

[<Fact>]
let ``reads from the stream until the first non number character is encountered``() =
    use stream = jsonStream "1.234567eE+-89xyz"
    let jsonNumber = new JsonNumber()

    jsonNumber.Read stream |> should equal "1.234567eE+-89"

[<Fact>]
let ``if the given stream is empty, the number buffer should be empty``() =
    use stream = jsonStream ""
    let jsonNumber = new JsonNumber()

    jsonNumber.Read stream |> should equal ""