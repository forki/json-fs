module ArrayTests

open Xunit
open FsUnit.Xunit
open JsonFs

[<Fact>]
let ``a empty array is parsed into an empty array``() =
    let result = Json.parse "[]"

    result |> should equal (Array [])

[<Fact>]
let ``an array containing a single type is parsed into an array``() =
    let result = Json.parse "[1, 2, 3]"

    result |> should equal (Array [Number 1M; Number 2M; Number 3M])

[<Fact>]
let ``an array containing multiple types is parsed into an array``() =
    let result = Json.parse "[true, false, null, 1, \"hello\", {\"a\": \"again\"}]"

    let expected =
        [("a", String "again")] |> Map.ofList

    result |> should equal 
        (Array 
            [Bool true;
             Bool false; 
             Null (); 
             Number 1M; 
             String "hello";
             Object expected])
