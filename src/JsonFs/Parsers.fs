namespace JsonFs

type Json = 
    | Bool of bool
    | Null of unit
    | Number of decimal
    | String of string

[<AutoOpen>]
module Parsers =
    open FParsec

    (* Grammar 

       For detailed information, please read RFC 7159, section 2
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

       For detailed information, please read RFC 7159, section 3
           See [https://tools.ietf.org/html/rfc7159#section-3] *)

    let private pboolean =
        stringReturn "true" true <|> stringReturn "false" false .>> pwhitespace

    let private pnull =
        stringReturn "null" () .>> pwhitespace

    (* Numbers 

       For detailed information, please read RFC 7159, section 6
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

    (* Strings 

       For detailed information, please read RFC 7159, section 7
           See [https://tools.ietf.org/html/rfc7159#section-7] *)

    [<RequireQualifiedAccess>]
    module private Escaping =
        open System
        open System.Globalization

        // The hexadecimal values are entries within the ASCII and UNICODE lookup tables
        [<Literal>]
        let private asciiSpace = 0x20
        [<Literal>]
        let private asciiExclamationMark = 0x21
        [<Literal>]
        let private asciiHash = 0x23
        [<Literal>]
        let private asciiLeftSquareBracket = 0x5b
        [<Literal>]
        let private asciiRightSquareBracket = 0x5d
        [<Literal>]
        let private unicodeSpecialBlockEnd = 0x10ffff

        let private unescaped c =
            c = asciiSpace || 
            c = asciiExclamationMark ||
            c >= asciiHash && c <= asciiLeftSquareBracket ||
            c >= asciiRightSquareBracket && c <= unicodeSpecialBlockEnd

        let private punescaped =
            satisfy (int >> unescaped)

        (* An escaped character can be represented by either uppercase or lowercase hexadecimal values *)

        [<Literal>]
        let private uppercaseA = 0x41
        [<Literal>]
        let private uppercaseF = 0x46
        [<Literal>]
        let private lowercaseA = 0x61
        [<Literal>]
        let private lowercaseF = 0x66

        let private hexdig i =
            (digit i)
            || (i >= uppercaseA && i <= uppercaseF)
            || (i >= lowercaseA && i <= lowercaseF) 

        let private p4hexdig =
            manyMinMaxSatisfy 4 4 (int >> hexdig)
                |>> fun str -> char (Int32.Parse(str, NumberStyles.HexNumber))

        let private pescaped =
            skipChar '\\' >>.
                choice [
                    skipChar '"'  >>% '\u0022'
                    skipChar '\\' >>% '\u005c'
                    skipChar '/'  >>% '\u002f'
                    skipChar 'b'  >>% '\u0008'
                    skipChar 'f'  >>% '\u000c'
                    skipChar 'n'  >>% '\u000a'
                    skipChar 'r'  >>% '\u000d'
                    skipChar 't'  >>% '\u0009'
                    skipChar 'u'  >>. p4hexdig ]

        let pchar =
            choice [ 
                punescaped 
                pescaped ]

        let parse =
            many pchar
 
    let private pquotationMark =
        skipChar '"'

    let private pescapedString =
        between pquotationMark pquotationMark Escaping.parse 
            |>> fun chars -> new string (List.toArray chars)

    (* As defined in the FParsec documentation, any recursive parsing needs
       to be forward declared. This will allow parsing of nested JSON elements *)
    
    let internal pjson, pjsonRef = createParserForwardedToRef()

    do pjsonRef := choice [ 
                pboolean        |>> Json.Bool
                pnull           |>> Json.Null
                pnumber         |>> Json.Number
                pescapedString  |>> Json.String
            ]
    
    [<RequireQualifiedAccess>]
    module Json =

        (* Utility functions for parsing JSON in its textual form *)

        let parse text =
            match run pjson text with
            | Success (json, _, _) ->  json
            | Failure (error, _, _) -> failwith error
