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

    let rec private formatJson options = 
        function
        | Null _ ->  "null"
        | Bool value -> formatBool value
        | _ -> ""

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