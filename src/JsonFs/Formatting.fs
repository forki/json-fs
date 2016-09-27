namespace JsonFs

[<AutoOpen>]
module Formatting =
    open System
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

    type FormattingOptions =
        {
            Spacing: StringBuilder -> StringBuilder
            NewLine: int -> StringBuilder -> StringBuilder
        }
        static member Compact =
            {
                Spacing = id
                NewLine = fun _ -> id
            }
        static member Indented =
            {
                Spacing = append " "
                NewLine = fun i -> append Environment.NewLine >> append (String.replicate i "  ")
            }

    let rec private formatJson level (options: FormattingOptions) = 
        function
        | Object value -> formatObject level options value
        | Array value -> formatArray level options value
        | String value -> formatString value
        | Number value -> formatNumber value
        | Bool value -> formatBool value
        | Null _ -> append "null"

    and private formatObject level options =
        function
        | value -> append "{" 
                   >> options.NewLine (level+1)
                   >> appendJoin (fun (k, v) -> appendFormat "\"{0}\":" k >> options.Spacing >> (formatJson (level+1) options v)) 
                        (append "," >> options.NewLine (level+1))
                        (Map.toList value)
                   >> options.NewLine level
                   >> append "}"

    and private formatArray level options =
        function
        | value -> append "["
                   >> options.NewLine (level+1)
                   >> appendJoin (formatJson (level+1) options) (append "," >> options.NewLine (level+1)) value
                   >> options.NewLine level
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

        let formatWith options json =
            StringBuilder()
            |> formatJson 0 options json
            |> string
    
        let format json =
            formatWith FormattingOptions.Compact json