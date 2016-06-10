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

    let pminus =
        charReturn '-' "-"

    let pplus =
        charReturn '+' "+"

    let pdecimal =
        charReturn '.' "."

    [<Literal>]
    let private e = 0x65
    [<Literal>]
    let private E = 0x45

    let private exponent i =
        i = e || i = E

    let private pint =
        pzero <|> (satisfy (int >> digit1to9) .>>. manySatisfy (int >> digit) 
            |>> fun (first, n) -> string first + n)

    let private (|??) =
        function
            | Some option -> option
            | _ -> ""

    let private pfraction =
        pdecimal .>>. manySatisfy (int >> digit) 
            |>> fun (decimal, i) -> decimal + i

    let private pexponent =
        pipe3 (satisfy (int >> exponent)) (opt (pminus <|> pplus)) (many1Satisfy (int >> digit))
            (fun exponent sign i -> string exponent + (|??) sign + i)

    let private pnumber =
        pipe4 (opt pminus) pint (opt pfraction) (opt pexponent)
            (fun sign i fraction exp -> decimal((|??) sign + i + (|??) fraction + (|??) exp))

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
