module AJsonProperty

open Xunit
open FsUnit.Xunit
open JsonStreamFactory
open JsonCs

let validJsonProperties : obj array seq =
    seq {
        yield [| "\"name\":"; "name" |]
        yield [| "\" name \" :"; " name " |]
    }

[<Theory>]
[<MemberData("validJsonProperties")>]
let ``a valid json property is correctly parsed``(value: string, expected: string) =
    use stream = jsonStream value
    let result = stream.ReadProperty()
    
    result |> should equal expected

let invalidJsonProperties : obj array seq =
    seq {
        yield [| "\"name:" |]
        yield [| "name\":" |]
        yield [| "\"name\"" |]
    }

[<Theory>]
[<MemberData("invalidJsonProperties")>]
let ``an invalid json property causes an exception to be thrown``(value: string) =
    use stream = jsonStream value
    (fun() -> stream.ReadProperty() |> ignore) |> should throw typeof<UnexpectedJsonException>