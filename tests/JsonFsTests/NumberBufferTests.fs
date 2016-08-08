module NumberBufferTests

open Xunit
open FsUnit.Xunit
open ParserCs;
open JsonStreamFactory

[<Fact>]
let ``reads the stream until the first non number character is encountered``() =
    use stream = jsonStream "1.234567eE+-89xyz"

    JsonNumber.FromStream(stream).ToString() |> should equal "1.234567eE+-89"

[<Fact>]
let ``if the given stream is empty, the number buffer should be empty``() =
    use stream = jsonStream ""

    let numberBuffer = JsonNumber.FromStream(stream)

    numberBuffer.BufferSize |> should equal 0
    numberBuffer.ToString() |> should equal ""