open System
open System.IO
open System.Diagnostics
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open JsonFs

type ParsingStatistics =
    {
        Iterations : int
        File : string
        SizeInBytes : int
        SizeInKilobytes : int
        Duration : TimeSpan
        Average : TimeSpan
        Parser : string option
    }
    override this.ToString() =
        let parserAsString = function
        | Some name -> name
        | None -> "Not Defined"

        sprintf "|%-15s|%-15s|%12i|%10i|%10i|%16O|%16O|" 
            (parserAsString this.Parser) this.File this.SizeInBytes this.SizeInKilobytes this.Iterations this.Duration this.Average

type Parser = string -> unit

let fileAsString =
    fun file -> File.ReadAllText file

let parseJson file (parser: Parser) iterations =
    let json = fileAsString file
    let sizeInBytes = Encoding.UTF8.GetByteCount json

    let sw = Stopwatch()
    sw.Start()

    for i = 1 to iterations do
        parser json

    sw.Stop()

    {
        Iterations = iterations; 
        File = file;
        SizeInBytes = sizeInBytes;
        SizeInKilobytes = sizeInBytes / 1024;
        Duration = sw.Elapsed;
        Average = TimeSpan.FromTicks(sw.ElapsedTicks / (int64 iterations));
        Parser = None
    }

let jsonFsParser =
    fun json -> Json.parse json |> ignore

let newtonsoftParser =
    fun json -> JObject.Parse(json) |> ignore

let parseWithNewtonsoft file iterations =
    let statistic = parseJson file newtonsoftParser iterations

    {statistic with Parser = Some("Newtonsoft.Json")}

let parseWithJsonFs file iterations =
    let statistic = parseJson file jsonFsParser iterations

    {statistic with Parser = Some("JsonFs")}

let parseFileAndCollectStatistics file iterations =
    printfn " * %s" file

    let newtonsoftStatistic = parseWithNewtonsoft file iterations
    let jsonFsStatistic = parseWithJsonFs file iterations
    
    (newtonsoftStatistic, jsonFsStatistic)

let collectParsingStatistics files =
    printfn "Parsing input files:"

    let rec parseJsonFile collectedStats = function
        | [] -> List.rev collectedStats
        | x::xs -> parseJsonFile ((parseFileAndCollectStatistics x 100000)::collectedStats) xs

    parseJsonFile [] files

let printStatisticsHeader() =
    printfn ""
    printfn "|%-15s|%-15s|%-12s|%-10s|%-10s|%-16s|%-16s|" 
        "Parser" "File" "Json (Bytes)" "Json (KBs)" "Iterations" "Total Time" "Average Time" 

let printStatistics statistics =
    printStatisticsHeader()

    let printStatistic = function
        | (a: ParsingStatistics, b: ParsingStatistics) -> printfn "%O\r\n%O" a b

    statistics |> List.iter printStatistic
    printfn ""

[<EntryPoint>]
let main argv = 
    // TODO: Generate better json samples

    [1..3] |> List.iter (fun i -> collectParsingStatistics ["small.json"; "medium.json"] |> printStatistics)
    0
