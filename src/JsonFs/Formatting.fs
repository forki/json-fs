namespace JsonFs

[<AutoOpen>]
module Formatting =
    open System.Text

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

    let rec private formatJson options = 
        function
        | Object value -> formatObject value
        | Array value -> formatArray value
        | String value -> formatString value
        | Number value -> formatNumber value
        | Bool value -> formatBool value
        | Null _ ->  "null"

    and private formatObject =
        function
        | value -> "" // TODO: need to loop over each element and call formatJson

    and private formatArray =
        function
        | value -> "" // TODO: need to loop over each element and call formatJson

    and private formatString =
        function
        | value -> sprintf "\"%s\"" value

    and private formatNumber =
        function
        | value -> sprintf "%E" value

    and private formatBool =
        function
        | true -> "true"
        | _ -> "false"

    [<RequireQualifiedAccess>]
    module Json =
    
        let formatWith =
            fun options json -> formatJson options json

        let format =
            fun json -> formatWith FormattingOptions.Compact json