namespace JsonFs

[<AutoOpen>]
module Formatting =
    open System.Text

    type private Formatter<'a> =
        'a -> StringBuilder -> StringBuilder

    type private Separator =
        StringBuilder -> StringBuilder

    let private append (text: string) (builder: StringBuilder) =
        builder.Append text

    let private appendFormat (text: string) (format: obj) (builder: StringBuilder) =
        builder.AppendFormat(text, format)

    let private appendJoin<'a> (formatter: Formatter<'a>) (separator: Separator) =
        let rec join values (builder: StringBuilder) =
            match values with
            | [] -> builder
            | [i] -> formatter i builder
            | head::tail -> (formatter head >> separator >> join tail) builder
        
        join

    let rec private formatJson = 
        function
        | Object value -> formatObject value
        | Array value -> formatArray value
        | String value -> formatString value
        | Number value -> formatNumber value
        | Bool value -> formatBool value
        | Null _ -> append "null"

    and private formatObject =
        function
        | value -> append "{" 
                   >> appendJoin (fun (k, v) -> appendFormat "\"{0}\":" k >> formatJson v) 
                        (append ",") 
                        (Map.toList value)
                   >> append "}"

    and private formatArray =
        function
        | value -> append "[" 
                   >> appendJoin formatJson (append ",") value
                   >> append "]"

    and private formatString =
        function
        | value -> appendFormat "\"{0}\"" value

    and private formatNumber =
        function
        | value -> append (string value)

    and private formatBool =
        function
        | true -> append "true"
        | _ -> append "false"

    [<RequireQualifiedAccess>]
    module Json =
    
        let format json =
            StringBuilder()
            |> formatJson json
            |> string