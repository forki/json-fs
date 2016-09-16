open System

let parseFileAndCollectStatistics file iterations =
    printfn " * %s" file

    let chironStatistic = Chiron.parseJson file iterations
    let newtonsoftStatistic = Newtonsoft.parseJson file iterations
    let jsonFsStatistic = JsonFs.parseJson file iterations
    
    [chironStatistic; newtonsoftStatistic; jsonFsStatistic]

let collectParsingStatistics files =
    printfn "Parsing input files:"

    let rec parseJsonFile collectedStats = function
        | [] -> collectedStats
        | x::xs -> parseJsonFile (List.append (parseFileAndCollectStatistics x 100000) collectedStats) xs

    parseJsonFile [] files

let printStatisticsHeader() =
    printfn ""
    printfn "|%-15s|%-12s|%-10s|%-16s|%-16s|" 
        "Parser" "Bytes" "Iterations" "Total Time" "Average Time" 

let printStatistics statistics =
    printStatisticsHeader()

    let printStatistic =
        fun statistic -> printfn "%O" statistic

    statistics |> List.iter printStatistic
    printfn ""

[<EntryPoint>]
let main argv = 
    [1..3] |> List.iter (fun i -> collectParsingStatistics ["small.json"; "medium.json"; "large.json"] |> printStatistics)
    0
