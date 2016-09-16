open System

let parseFileAndCollectStatistics file iterations =
    printfn " * %s" file

    let newtonsoftStatistic = Newtonsoft.parseJson file iterations
    let chironStatistic = Chiron.parseJson file iterations
    let jsonFsStatistic = JsonFs.parseJson file iterations
    
    // TODO: convert this to list

    (newtonsoftStatistic, chironStatistic, jsonFsStatistic)

let collectParsingStatistics files =
    printfn "Parsing input files:"

    let rec parseJsonFile collectedStats = function
        | [] -> List.rev collectedStats
        | x::xs -> parseJsonFile ((parseFileAndCollectStatistics x 100000)::collectedStats) xs

    parseJsonFile [] files

let printStatisticsHeader() =
    printfn ""
    printfn "|%-15s|%-12s|%-10s|%-16s|%-16s|" 
        "Parser" "Bytes" "Iterations" "Total Time" "Average Time" 

let printStatistics statistics =
    printStatisticsHeader()

    // TODO: convert this to list

    let printStatistic = function
        | (a: ParsingStatistics, b: ParsingStatistics, c: ParsingStatistics) -> printfn "%O\r\n%O\r\n%O" a b c

    statistics |> List.iter printStatistic
    printfn ""

[<EntryPoint>]
let main argv = 
    // TODO: Generate better json samples

    [1..3] |> List.iter (fun i -> collectParsingStatistics ["small.json"; "medium.json"] |> printStatistics)
    0
