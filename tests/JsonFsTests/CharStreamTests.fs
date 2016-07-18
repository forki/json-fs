module CharStreamTests

open Xunit
open FsUnit.Xunit
open System.IO
open ParserCs

let buildCharStream str =
    new CharStream(new StringReader(str))

[<Fact>]
let ``when peeking the next expected character should be returned``() =
    use charStream = buildCharStream "a"

    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping advances the reader if the character sequence matches``() =
    use charStream = buildCharStream "abcd"

    charStream.Skip("abc") |> should equal true
    charStream.Peek() |> should equal 'd'

[<Fact>]
let ``skipping will not advance if the character sequence does not match``() =
    use charStream = buildCharStream "abcd"

    charStream.Skip("e") |> should equal false
    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping will not advance if character sequence is null``() =
    use charStream = buildCharStream "abcd"

    charStream.Skip(null) |> should equal true
    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skipping will not advance if character sequence is of 0 length``() =
    use charStream = buildCharStream "abcd"

    charStream.Skip("") |> should equal true
    charStream.Peek() |> should equal 'a'

[<Fact>]
let ``skips all whitespace until a non whitespace character is reached``() =
    use charStream = buildCharStream " \t\r\ndef"

    charStream.SkipWhitespace()
    charStream.Peek() |> should equal 'd'

[<Fact>]
let ``skips all whitespace until end of character sequence reached``() =
    use charStream = buildCharStream " \t\r\n"

    charStream.SkipWhitespace()
    charStream.Peek() |> should equal '\u0000'

[<Fact>]
let ``reading a single character should advance the stream``() =
    use charStream = buildCharStream "ab"

    charStream.Read() |> should equal 'a'
    charStream.Peek() |> should equal 'b'

[<Fact>]
let ``reading a sequence of characters should advance the stream``() =
    use charStream = buildCharStream "abcd"

    charStream.Read(3u) |> should equal [|'a'; 'b'; 'c'|]
    charStream.Peek() |> should equal 'd'