module CharStreamTests

open Xunit
open FsUnit.Xunit
open System.IO
open ParserCs

let charStream text =
    new CharStream(new StringReader(text))

let charStreamWithBufferSize text bufferSize =
    new CharStream(new StringReader(text), bufferSize)

[<Fact>]
let ``when peeking the next expected character should be returned``() =
    use charStream = charStream "a"

    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping advances the reader if the character sequence matches``() =
    use charStream = charStream "abcd"

    charStream.Skip("abc") |> should equal true
    charStream.Peek() |> should equal 'd'

[<Fact>]
let ``skipping will not advance if the character sequence does not match``() =
    use charStream = charStream "abcd"

    charStream.Skip("e") |> should equal false
    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping will not advance if character sequence is null``() =
    use charStream = charStream "abcd"

    charStream.Skip(null) |> should equal true
    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping will not advance if character sequence is of 0 length``() =
    use charStream = charStream "abcd"

    charStream.Skip("") |> should equal true
    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skips all whitespace until a non whitespace character is reached``() =
    use charStream = charStream " \t\r\ndef"

    charStream.SkipWhitespace()
    charStream.Peek() |> should equal 'd'

[<Fact>]
let ``skips all whitespace until end of character sequence reached``() =
    use charStream = charStream " \t\r\n"

    charStream.SkipWhitespace()
    charStream.Peek() |> should equal '\u0000'

[<Fact>]
let ``reading a single character should advance the stream``() =
    use charStream = charStream "ab"

    charStream.Read() |> should equal 'a'
    charStream.Peek() |> should equal 'b'

[<Fact>]
let ``reading a number of characters should advance the stream``() =
    use charStream = charStream "abcd"

    charStream.Read(3u) |> should equal [|'a'; 'b'; 'c'|]
    charStream.Peek() |> should equal 'd'

[<Fact>]
let ``reading a single character when at the end of the stream will trigger a buffer reload``() =
    use charStream = charStreamWithBufferSize "ab" 1

    charStream.Read() |> should equal 'a'
    charStream.Read() |> should equal 'b'

[<Fact>]
let ``reading a number of characters when at the end of the stream will trigger a buffer reload``() =
    use charStream = charStreamWithBufferSize "abcdefghij" 5

    charStream.Read(5u) |> should equal [| 'a'; 'b'; 'c'; 'd'; 'e' |]
    charStream.Read(5u) |> should equal [| 'f'; 'g'; 'h'; 'i'; 'j' |]

[<Fact>]
let ``after a buffer reload, the buffer should be correctly null terminated``() =
    use charStream = charStreamWithBufferSize "abcd" 3

    charStream.Read(4u) |> ignore
    charStream.Peek() |> should equal '\u0000'
