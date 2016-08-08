﻿module JsonStreamTests

open Xunit
open FsUnit.Xunit
open ParserCs
open JsonStreamFactory

[<Fact>]
let ``when peeking the next expected character should be returned``() =
    use stream = jsonStream "a"

    stream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping advances the reader if the character sequence matches``() =
    use stream = jsonStream "abcd"

    stream.Skip("abc") |> should equal true
    stream.Peek() |> should equal 'd'

[<Fact>]
let ``skipping will not advance if the character sequence does not match``() =
    use stream = jsonStream "abcd"

    stream.Skip("e") |> should equal false
    stream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping will not advance if character sequence is null``() =
    use stream = jsonStream "abcd"

    stream.Skip(null) |> should equal true
    stream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping will not advance if character sequence is of 0 length``() =
    use stream = jsonStream "abcd"

    stream.Skip("") |> should equal true
    stream.Peek() |> should equal 'a'

[<Fact>]
let ``skips all whitespace until a non whitespace character is reached``() =
    use stream = jsonStream " \t\r\ndef"

    stream.SkipWhitespace()
    stream.Peek() |> should equal 'd'

[<Fact>]
let ``skips all whitespace until end of character sequence reached``() =
    use stream = jsonStream " \t\r\n"

    stream.SkipWhitespace()
    stream.Peek() |> should equal '\u0000'

[<Fact>]
let ``reading a single character should advance the stream``() =
    use stream = jsonStream "ab"

    stream.Read() |> should equal 'a'
    stream.Peek() |> should equal 'b'

[<Fact>]
let ``reading a single character when at the end of the stream will trigger a buffer reload``() =
    use stream = jsonStreamWithBufferSize "ab" 1

    stream.Read() |> should equal 'a'
    stream.Read() |> should equal 'b'

[<Fact>]
let ``while skipping a sequence of characters, if the end of stream is reached, a buffer reload is triggered``() =
    use stream = jsonStreamWithBufferSize "abcd" 3

    stream.Skip("abcd") |> should equal true
    stream.Peek() |> should equal '\u0000'

[<Fact>]
let ``while skipping whitespace, if the end of stream is reached, a buffer reload is triggered``() =
    use stream = jsonStreamWithBufferSize " \t\r\n" 3
    
    stream.SkipWhitespace()
    stream.Peek() |> should equal '\u0000'

[<Fact>]
let ``reading a single character, while at the end of the buffer, will always return the null terminator``() =
    use stream = jsonStreamWithBufferSize "a" 2

    stream.Skip("a") |> ignore
    stream.Read() |> should equal '\u0000'
    stream.Read() |> should equal '\u0000'

[<Fact>]
let ``skipping while at the end of the buffer, will always return false``() =
    use stream = jsonStreamWithBufferSize "abc" 2

    stream.Skip("abc") |> should equal true
    stream.Skip("d") |> should equal false

[<Fact>]
let ``when skipping over a buffer boundary, if a match fails, the read position should be reset``() =
    use stream = jsonStreamWithBufferSize "abcdef" 3

    stream.Skip("abcdf") |> should equal false
    stream.Peek() |> should equal 'a'

[<Fact>]
let ``reading multiple characters should advance the stream``() =
    use stream = jsonStream "abcdef"

    stream.Read 4 |> should equal [| 'a'; 'b'; 'c'; 'd' |]

[<Fact>]
let ``when reading multiple characters, a padded array is returned if there are not enough characters in the stream``() =
    use stream = jsonStream "abc"

    stream.Read 4 |> should equal [| 'a'; 'b'; 'c'; '\u0000' |]

[<Fact>]
let ``an UnexpectedJsonException is thrown if an expected character doesn't match``() =
    use stream = jsonStream "a"
    (fun() -> stream.Expect 'b') |> should throw typeof<UnexpectedJsonException>
