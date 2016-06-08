namespace JsonFs

type Json = 
    | Bool of bool
    | Null of unit

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

    (* As defined in the FParsec documentation, any recursive parsing needs
       to be forward declared. This will allow parsing of nested JSON elements *)
    
    let internal pjson, pjsonRef = createParserForwardedToRef()

    do pjsonRef := choice [ 
                pboolean |>> Bool
                pnull    |>> Null
            ]
    
    [<RequireQualifiedAccess>]
    module Json =

        (* Utility functions for parsing JSON in its textual form *)

        let parse text =
            match run pjson text with
            | Success (json, _, _) ->  json
            | Failure (error, _, _) -> failwith error
