module ObjectTests

open Xunit
open FsUnit.Xunit
open JsonFs

[<Fact>]
let ``an empty object is parsed into an empty map``() =
    let result = Json.parse "{}"

    result |> should equal (Json.Object Map.empty<string, Json>)

[<Fact>]
let ``an empty object is parsed into an empty map when trailed by whitespace``() =
    let result = Json.parse "{}    "

    result |> should equal (Json.Object Map.empty<string, Json>)

[<Fact>]
let ``a simple object is parsed into a map with the correct keys and values``() =
    let result = Json.parse "{\"a\": true, \"b\": null, \"c\": \"hello\", \"d\": 1, \"e\": [1]}"

    let expected = 
        [("a", Json.Bool true); 
         ("b", Json.Null ()); 
         ("c", Json.String "hello"); 
         ("d", Json.Number 1M);
         ("e", Json.Array [Json.Number 1M])] |> Map.ofList

    result |> should equal (Json.Object expected)

[<Fact>]
let ``a nested object is parsed into a nested map with the correct keys and values``() =
    let result = Json.parse "{\"a\": {\"b\": \"I'm nested\"}}"

    let nested =
        [("b", Json.String "I'm nested")] |> Map.ofList

    let expected = 
        [("a", Json.Object nested)] |> Map.ofList

    result |> should equal (Json.Object expected)

[<Fact>]
let ``a simple object interspersed with whitespace is parsed into a map with the correct keys and values``() =
    let result = Json.parse "{ \"a\" : true , \"b\" :  null  ,  \"c\"  : \"hello\" , \"d\" : 1  , \"e\" : [1]   }"

    let expected = 
        [("a", Json.Bool true); 
         ("b", Json.Null ()); 
         ("c", Json.String "hello"); 
         ("d", Json.Number 1M);
         ("e", Json.Array [Json.Number 1M])] |> Map.ofList

    result |> should equal (Json.Object expected)