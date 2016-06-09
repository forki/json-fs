namespace JsonFs

type Json = 
    | Bool of bool
    | Null of unit
    | Number of decimal

[<AutoOpen>]
module Parsers =
    open FParsec

    (* Grammar 
       
       Common grammatical elements that define the structure within JSON text,
       as defined by the RFC 7159 standard.

       For further detailed information, please read:
           RFC 7159, Section 2; JSON Grammar
           See [https://tools.ietf.org/html/rfc7159#section-2] *)

    [<Literal>]
    let private space = 0x20
    [<Literal>]
    let private horizontalTab = 0x09
    [<Literal>]
    let private lineFeed = 0x0A
    [<Literal>]
    let private carriageReturn = 0x0D

    let private whitespace char = 
        char = space || char = horizontalTab || char = lineFeed || char = carriageReturn

    let private pwhitespace =
        skipManySatisfy (int >> whitespace)

    (* Values
       
       Three literal values that are supported within JSON text, as defined by
       the RFC 7159 standard.

       For further detailed information, please read:
           RFC 7159, Section 3; Values
           See [https://tools.ietf.org/html/rfc7159#section-3] *)

    let private pboolean =
        stringReturn "true" true <|> stringReturn "false" false .>> pwhitespace

    let private pnull =
        stringReturn "null" () .>> pwhitespace

    (* Numbers 
    
       Numerical representations of numbers that are supported within JSON text,
       as defined by the RFC 7159 standard.
       
       For further detailed information, please read:
           RFC 7159, Section 6; Number
           See [https://tools.ietf.org/html/rfc7159#section-6] *)

    [<Literal>]
    let private zero = 0x30
    [<Literal>]
    let private one = 0x31
    [<Literal>]
    let private nine = 0x39

    let private digit1to9 i = 
        i >= one && i <= nine

    let private digit i =
        digit1to9 i || i = zero

    let private pzero =
        charReturn '0' "0"

    (* An int is only valid if:
         a. it has the value 0
         b. or, it starts with a digit between 1 and 9 and then is followed by n digits
         c. an optional minus sign can be used as a prefix *)

    let pminus =
        charReturn '-' "-"

    let private pint =
        pzero <|> (satisfy (int >> digit1to9) .>>. manySatisfy (int >> digit) 
            |>> fun (first, n) -> string first + n)

    let private (|??) =
        function
            | Some option -> option
            | _ -> ""

    let private pnumber =
        pipe2 (opt(pminus)) pint (fun sign i -> decimal((|??) sign + i))

    (* As defined in the FParsec documentation, any recursive parsing needs
       to be forward declared. This will allow parsing of nested JSON elements *)
    
    let internal pjson, pjsonRef = createParserForwardedToRef()

    do pjsonRef := choice [ 
                pboolean |>> Json.Bool
                pnull    |>> Json.Null
                pnumber  |>> Json.Number
            ]
    
    [<RequireQualifiedAccess>]
    module Json =

        (* Utility functions for parsing JSON in its textual form *)

        let parse text =
            match run pjson text with
            | Success (json, _, _) ->  json
            | Failure (error, _, _) -> failwith error
