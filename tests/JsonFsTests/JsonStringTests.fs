module JsonStringTests

open Xunit
open FsUnit.Xunit
open JsonCs
open JsonStreamFactory

[<Fact>]
let ``reads from the stream until the first unescaped double quotation mark``() =
    use stream = jsonStream @"abc""defg"
    let jsonString = new JsonString()

    jsonString.Read stream |> should equal "abc"

[<Fact>]
let ``reads from the stream until the first null terminator``() =
    use stream = jsonStream "abc\u0000def"
    let jsonString = new JsonString()

    jsonString.Read stream |> should equal "abc"

[<Fact>]
let ``expands internal buffer to ensure entire string is read from the stream``() =
    let text = String.replicate 1025 "a"

    use stream = jsonStream text
    let jsonString = new JsonString()

    jsonString.Read stream |> should equal text