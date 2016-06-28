module ArrayTests

open Xunit
open FsUnit.Xunit
open JsonFs

[<Fact>]
let ``a empty array is parsed into an empty array``() =
    let result = Json.parse "[]"

    result |> should equal (Json.Array [])

[<Fact>]
let ``an array containing a single type is parsed into an array``() =
    let result = Json.parse "[1, 2, 3]"

    result |> should equal (Json.Array [Json.Number 1M; Json.Number 2M; Json.Number 3M])

[<Fact>]
let ``an array containing multiple types is parsed into an array``() =
    let result = Json.parse "[true, null, 1, \"hello\"]"

    result |> should equal (Json.Array [Json.Bool true; Json.Null (); Json.Number 1M; Json.String "hello"])