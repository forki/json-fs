open System.Diagnostics
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open JsonFs

let simpleJson = "{\"FirstName\": \"John\", \"LastName\": \"Doe\", \"DOB\": \"21/03/1984\", \"Age\": 32, \"Occupation\": \"Line Manager\"}"

let parseSimpleJsonWithNewtonsoft =
    let sw = Stopwatch()
    sw.Start()

    for i = 1 to 1000 do
        JObject.Parse(simpleJson) |> ignore

    sw.Stop()
    printfn "Parsing 1000 simple documents with Newtonsoft took: %O" sw.Elapsed

let parseSimpleJsonWithJsonFs =
    let sw = Stopwatch()
    sw.Start()

    for i = 1 to 1000 do
        Json.parse simpleJson |> ignore

    sw.Stop()
    printfn "Parsing 1000 simple documents with JsonFs took: %O" sw.Elapsed

[<EntryPoint>]
let main argv = 
    
    // Parse using a simple Json document first
    parseSimpleJsonWithNewtonsoft
    parseSimpleJsonWithNewtonsoft

    0
