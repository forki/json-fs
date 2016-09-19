module NumberTests

open Xunit
open FsUnit.Xunit
open JsonFs
open JsonCs
open System

let numberValues : obj array seq =
    seq {
        yield [| "0"; Number 0M |]
        yield [| "01"; Number 1M |]
        yield [| "123456789"; Number 123456789M |]
        yield [| "-1"; Number -1M |]
        yield [| "+1"; Number 1M |]
        yield [| "-0"; Number 0M |]
        yield [| "3.14519"; Number 3.14519M |]
        yield [| "3."; Number 3M |]
        yield [| ".321"; Number 0.321M |]
        yield [| "1.2345E-02"; Number 1.2345E-02M |]
        yield [| "1.2345e+02"; Number 1.2345e+02M |]
        yield [| "1.2345e02"; Number 1.2345e02M |]
        yield [| "123e02M"; Number 123e02M |]
    }

[<Theory>]
[<MemberData("numberValues")>]
let ``the numeric string is correctly parsed into a JSON ast node``(value: string, expected: Json) =
    let result = Json.parse value

    result |> should equal expected

let invalidNumberValues : obj array seq =
    seq {
        yield [| "1.2345e" |]
        yield [| "1.2345E+" |]
        yield [| "1..0" |]
        yield [| "1.2345+e02" |]
        yield [| "1.2345-E02" |]
        yield [| "+-1" |]
    }

[<Theory>]
[<MemberData("invalidNumberValues")>]
let ``the numeric string must be parsed into a valid number otherwise an exception is thrown``(value: string) =
    (fun() -> Json.parse value |> ignore) |> should throw typeof<UnexpectedJsonException>
