namespace JsonFs

[<AutoOpen>]
module Parsers =
    open JsonCs

    (* JSON grammar as defined within the RFC7159 specification
       
       See: https://tools.ietf.org/html/rfc7159#section-2 for more details *)

    [<Literal>]
    let private beginArray = '['
    [<Literal>]
    let private endArray = ']'
    [<Literal>]
    let private beginObject = '{'
    [<Literal>]
    let private endObject = '}'
    [<Literal>]
    let private valueSeparator = ','
    [<Literal>]
    let private nameSeparator = ':'
    [<Literal>]
    let private quotationMark = '"'

    (* By forward declaring the parser, dependency ordering within the module disappears  *)
    
    type private Parser<'t> = JsonStream -> 't

    let createForwardDeclaredParser() =
        let parser = fun stream -> failwith "the parser has not yet been initialised"
        parser : Parser<'t>

    let mutable private pjson = createForwardDeclaredParser()

    let private parseNumber (stream: JsonStream) =
        try
            let number = JsonNumber.FromStream(stream).ToString()
            decimal number
        with
        | :? System.FormatException -> raise (UnexpectedJsonException());

    let private parseString (stream: JsonStream) =        
        stream.Expect quotationMark
        let jsonString = JsonString.FromStream(stream).ToString()
        stream.Expect quotationMark

        jsonString

    let emptyBetween (startChar: char) (endChar: char) (stream: JsonStream) =
        let mutable empty = false
        
        if stream.Skip startChar then
            stream.SkipWhitespace()

            if stream.Skip endChar then
                empty <- true

        empty

    let emptyArray =
        fun stream -> emptyBetween beginArray endArray stream

    let emptyObject =
        fun stream -> emptyBetween beginObject endObject stream
        
    let private parseArray (stream: JsonStream) =
        let mutable jsonArray = []

        if not (emptyArray stream) then
            stream.SkipWhitespace()
            jsonArray <- [pjson stream] // this operator is the argument
            stream.SkipWhitespace()

            while stream.Skip valueSeparator do
                stream.SkipWhitespace()
                jsonArray <- (pjson stream)::jsonArray // this operator is the argument
                stream.SkipWhitespace()

            stream.Expect endArray

        List.rev jsonArray

    let private parseObject (stream: JsonStream) =
        let mutable jsonObject = []

        if not (emptyObject stream) then
            stream.SkipWhitespace()
            let property = parseString stream
            
            stream.SkipWhitespace()
            stream.Skip nameSeparator |> ignore
            stream.SkipWhitespace()

            let value = pjson stream
            stream.SkipWhitespace()

            jsonObject <- [property, value] // this operator is the argument

            while stream.Skip valueSeparator do
                stream.SkipWhitespace()
                let property = parseString stream

                stream.SkipWhitespace()
                stream.Skip nameSeparator |> ignore
                stream.SkipWhitespace()

                let value = pjson stream
                stream.SkipWhitespace()

                jsonObject <- (property, value)::jsonObject // this operator is the argument
                ()

            stream.Expect endObject
            
        Map.ofList (List.rev jsonObject)

    [<RequireQualifiedAccess>]
    module Json =
        open System.IO

        let internal jsonObject =
            fun stream -> parseObject stream |> Object

        let internal jsonArray =
            fun stream -> parseArray stream |> Array

        let internal jsonString =
            fun stream -> parseString stream |> String

        let internal jsonNumber =
            fun stream -> parseNumber stream |> Number

        let private parseJson (stream: JsonStream) =
            match stream.Peek() with
            | '{' -> jsonObject stream
            | '[' -> jsonArray stream
            | '"' -> jsonString stream
            | 't' when stream.Skip("true") -> Bool true
            | 'f' when stream.Skip("false") -> Bool false
            | 'n' when stream.Skip("null") -> Null ()
            | _ -> jsonNumber stream

        do pjson <- parseJson

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        let parse json =
            use stream = new JsonStream(new StringReader(json))      
            pjson stream
