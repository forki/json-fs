module LiteralValueTests

open Xunit
open FsUnit.Xunit
open JsonFs
open JsonCs
open System

let literalValues : obj array seq =
    seq {
        yield [| "true"; Bool true |]
        yield [| "false"; Bool false|]
        yield [| "null"; Null () |]
    }

[<Theory>]
[<MemberData("literalValues")>]
let ``the literal string is correctly parsed into a JSON ast node``(value: string, expected: Json) =
    let result = Json.parse value

    result |> should equal expected

let invalidLiteralValues : obj array seq =
    seq {
        yield [| "True" |]
        yield [| "False" |]
        yield [| "Null" |]
    }

[<Theory>]
[<MemberData("invalidLiteralValues")>]
let ``the literal string must be in lowercase when parsed otherwise an exception is thrown``(value: string) =
    (fun() -> Json.parse value |> ignore) |> should throw typeof<UnexpectedJsonException>
