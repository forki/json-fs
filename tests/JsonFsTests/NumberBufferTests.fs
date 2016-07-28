module NumberBufferTests

open Xunit
open FsUnit.Xunit
open ParserCs;
open CharStreamFactory

[<Fact>]
let ``reads the stream until the first non number character is encountered``() =
    use charStream = charStream "1.234567eE+-89xyz"

    NumberBuffer.FromStream(charStream).ToString() |> should equal "1.234567eE+-89"

[<Fact>]
let ``if the given stream is empty, the number buffer should be empty``() =
    use charStream = charStream ""

    let numberBuffer = NumberBuffer.FromStream(charStream)

    numberBuffer.BufferSize |> should equal 0
    numberBuffer.ToString() |> should equal ""