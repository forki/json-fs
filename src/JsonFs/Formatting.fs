namespace JsonFs

[<AutoOpen>]
module Formatting =
    open System.Text

    type private Formatter<'a> =
        'a -> StringBuilder -> StringBuilder

    let private append (text: string) (builder: StringBuilder) =
        builder.Append text

    let private appendf (text: string) (format: obj) (builder: StringBuilder) =
        builder.AppendFormat(text, format)

    type FormattingOptions =
        {
            Spacing: StringBuilder -> StringBuilder
        }
        static member Compact =
            {
                Spacing = id
            }
        static member SingleLine =
            {
                Spacing = id
            }
        static member Indented =
            {
                Spacing = id
            }

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
        | value -> appendf "{{{0}}}" ""

    and private formatArray =
        function
        | value -> appendf "[{0}]" ""

    and private formatString =
        function
        | value -> appendf "\"{0}\"" value

    and private formatNumber =
        function
        // TODO: need to support formatting of different numeric types
        | value -> append (string value)

    and private formatBool =
        function
        | true -> append "true"
        | _ -> append "false"

//    and private join list =
//        let rec join collected =
//            function
//            | [] -> ""
//            | x::xs -> join (formatJson x::xs) collected
//
//        join [] list

    [<RequireQualifiedAccess>]
    module Json =
    
        let formatWith options json =
            StringBuilder()
            |> formatJson json
            |> string

        let format =
            fun json -> formatWith FormattingOptions.Compact json