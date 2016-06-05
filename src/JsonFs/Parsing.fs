namespace JsonFs

module Parsing =
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

    let private isWhitespace char = 
        char = space || char = horizontalTab || char = lineFeed || char = carriageReturn

    let private pwhitespace =
        skipManySatisfy (int >> isWhitespace)

    let private pbeginArray =
        pwhitespace .>> skipChar '[' .>> pwhitespace

    let private pbeginObject =
        pwhitespace .>> skipChar '{' .>> pwhitespace

    let private pendArray =
        pwhitespace .>> skipChar ']' .>> pwhitespace

    let private pendObject =
        pwhitespace .>> skipChar '}' .>> pwhitespace

    let private pnameSeperator =
        pwhitespace .>> skipChar ':' .>> pwhitespace

    let private valueSeperator =
        pwhitespace .>> skipChar ',' .>> pwhitespace

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
