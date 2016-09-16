[<AutoOpen>]
module Parsers

open System
open System.IO
open System.Diagnostics
open System.Text

type ParsingStatistics =
    {
        Iterations : int
        SizeInBytes : int
        Duration : TimeSpan
        Average : TimeSpan
        Parser : string option
    }
    override this.ToString() =
        let parserAsString = function
        | Some name -> name
        | None -> "Not Defined"

        sprintf "|%-15s|%12i|%10i|%16O|%16O|" 
            (parserAsString this.Parser) this.SizeInBytes this.Iterations this.Duration this.Average

type Parser = string -> unit

let private fileAsString =
    fun file -> File.ReadAllText file

let private parseJson file (parser: Parser) iterations =
    let json = fileAsString file
    let sizeInBytes = Encoding.UTF8.GetByteCount json

    let sw = Stopwatch()
    sw.Start()

    for i = 1 to iterations do
        parser json

    sw.Stop()

    {
        Iterations = iterations; 
        SizeInBytes = sizeInBytes;
        Duration = sw.Elapsed;
        Average = TimeSpan.FromTicks(sw.ElapsedTicks / (int64 iterations));
        Parser = None
    }

[<RequireQualifiedAccess>]
module JsonFs =
    open JsonFs
        
    let private parse =
        fun json -> Json.parse json |> ignore


    let parseJson file iterations =
        let statistic = parseJson file parse iterations

        {statistic with Parser = Some("JsonFs")}

[<RequireQualifiedAccess>]
module Chiron =
    open Chiron

    let private parse =
        fun json -> Json.parse json |> ignore

    let parseJson file iterations =
        let statistic = parseJson file parse iterations

        {statistic with Parser = Some("Chiron")}

[<RequireQualifiedAccess>]
module Newtonsoft =
    open Newtonsoft
    open Newtonsoft.Json.Linq

    let private parse =
        fun json -> JObject.Parse(json) |> ignore

    let parseJson file iterations =
        let statistic = parseJson file parse iterations

        {statistic with Parser = Some("Newtonsoft.Json")}