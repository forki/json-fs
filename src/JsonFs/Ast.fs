namespace JsonFs

[<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
type Json = 
    | Bool of bool
    | Null of unit
    | Number of decimal
    | String of string
    | Array of Json list
    | Object of Map<string, Json>
    with
        member private this.StructuredFormatDisplay =
            match this with
            | String s -> box ("\"" + s + "\"")
            | Number f -> box f
            | Bool b -> box b
            | Null n -> box "null"
            | Array l -> box l
            | Object m -> Map.toList m :> obj
        override this.ToString() = sprintf "%A" this