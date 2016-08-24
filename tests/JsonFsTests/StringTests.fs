module StringTests

open Xunit
open FsUnit.Xunit
open JsonFs
open JsonCs

let wrap =
    fun str -> sprintf "\"%s\"" str

let escapedValues : obj array seq =
    seq {
        yield [| @"\"""; String "\u0022" |]
        yield [| @"\\"; String "\u005c" |]
        yield [| @"\/"; String "\u002f" |]
        yield [| @"\b"; String "\u0008" |]
        yield [| @"\f"; String "\u000c" |]
        yield [| @"\n"; String "\u000a" |]
        yield [| @"\r"; String "\u000d" |]
        yield [| @"\t"; String "\u0009" |]
        yield [| "\u0aE0"; String "ૠ" |]
    }

[<Theory>]
[<MemberData("escapedValues")>]
let ``an escaped string is correctly parsed into its unicode representation``(value: string, expected: Json) =
    let result = Json.parse (wrap value)

    result |> should equal expected

let invalidEscapedValues : obj array seq =
    seq {
        yield [| "abc\d" |]
        yield [| "\ua00z" |]
    }

[<Theory>]
[<MemberData("invalidEscapedValues")>]
let ``an unrecognised escape sequence when parsed will throw an exception``(value: string) =
    (fun() -> Json.parse (wrap value) |> ignore) |> should throw typeof<UnexpectedJsonException>

[<Fact>]
let ``a string containing the full range of ASCII characters is parsed without escaping``() =
    let result = Json.parse "\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~ !\""
    
    result |> should equal (Json.String "#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~ !")

[<Fact>]
let ``a string containing characters upto the end of the basic multilingual plane are parsed without escaping``() =
    let result = Json.parse "\"གྷᡵヶ⢇𐿿\""

    result |> should equal (Json.String "གྷᡵヶ⢇𐿿")
