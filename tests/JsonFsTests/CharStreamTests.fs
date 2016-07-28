module CharStreamTests

open Xunit
open FsUnit.Xunit
open ParserCs
open CharStreamFactory

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
let ``reading a single character when at the end of the stream will trigger a buffer reload``() =
    use charStream = charStreamWithBufferSize "ab" 1

    charStream.Read() |> should equal 'a'
    charStream.Read() |> should equal 'b'

[<Fact>]
let ``while skipping a sequence of characters, if the end of stream is reached, a buffer reload is triggered``() =
    use charStream = charStreamWithBufferSize "abcd" 3

    charStream.Skip("abcd") |> should equal true
    charStream.Peek() |> should equal '\u0000'

[<Fact>]
let ``while skipping whitespace, if the end of stream is reached, a buffer reload is triggered``() =
    use charStream = charStreamWithBufferSize " \t\r\n" 3
    
    charStream.SkipWhitespace()
    charStream.Peek() |> should equal '\u0000'

[<Fact>]
let ``reading a single character, while at the end of the buffer, will always return the null terminator``() =
    use charStream = charStreamWithBufferSize "a" 2

    charStream.Skip("a") |> ignore
    charStream.Read() |> should equal '\u0000'
    charStream.Read() |> should equal '\u0000'

[<Fact>]
let ``skipping while at the end of the buffer, will always return false``() =
    use charStream = charStreamWithBufferSize "abc" 2

    charStream.Skip("abc") |> should equal true
    charStream.Skip("d") |> should equal false

[<Fact>]
let ``when skipping over a buffer boundary, if a match fails, the read position should be reset``() =
    use charStream = charStreamWithBufferSize "abcdef" 3

    charStream.Skip("abcdf") |> should equal false
    charStream.Peek() |> should equal 'a'
