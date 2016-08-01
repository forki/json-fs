namespace JsonFs

type Json = 
    | Bool of bool
    | Null of unit
    | Number of decimal
    | String of string
    | Array of Json list
    | Object of Map<string, Json>

[<AutoOpen>]
module Parsers =
    open ParserCs

    type Parser<'u> = CharStream -> 'u

    let createParserForwardedToRef() =
        let dummyParser = fun stream -> failwith "a parser created with createParserForwardedToRef was not initialized"
        let r = ref dummyParser
        (fun stream -> !r stream), r : Parser<'u> * Parser<'u> ref

    let pjson, pjsonRef = createParserForwardedToRef()

    exception UnrecognisedJsonException of string

    let private parseNumber (stream: CharStream) =
        try
            let number = NumberBuffer.FromStream(stream).ToString()
            Number (decimal number)
        with
        | :? System.FormatException -> raise (UnrecognisedJsonException "invalid number");

    let private parseString (stream: CharStream) =        
        if not (stream.Skip("\"")) then
            raise (UnrecognisedJsonException "expecting a \" at the beginning of the string")

        let jsonString = StringBuffer.FromStream(stream).ToString()
        
        if not (stream.Skip("\"")) then
            raise (UnrecognisedJsonException "expecting a \" at the end of the string")

        String jsonString

    let private parseArray (stream: CharStream) =
        if not (stream.Skip("[")) then
            raise (UnrecognisedJsonException "expecting a [ at the beginning of the array")

        stream.SkipWhitespace()

        let mutable jsonArray = []

        if not (stream.Skip("]")) then
            jsonArray <- [pjson stream]
            stream.SkipWhitespace()

            while stream.Skip(",") do
                stream.SkipWhitespace()
                jsonArray <- (pjson stream)::jsonArray
                stream.SkipWhitespace()

            if not (stream.Skip("]")) then
                raise (UnrecognisedJsonException "expecting a ] at the end of the array")

        Array (List.rev jsonArray)

    [<RequireQualifiedAccess>]
    module Json =
        open System.IO

        let private parseJson (stream: CharStream) =
            match stream.Peek() with
            | '[' -> parseArray stream
            | '"' -> parseString stream
            | 't' when stream.Skip("true") -> Bool true
            | 'f' when stream.Skip("false") -> Bool false
            | 'n' when stream.Skip("null") -> Null ()
            | _ -> parseNumber stream

        pjsonRef := parseJson

        let parse json =
            use stream = new CharStream(new StringReader(json))      
            pjson stream
