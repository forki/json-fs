﻿namespace JsonFs

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
    
    (* By declaring these parsers once, memory allocations will be reduced as internal buffers reused *)
    let private jsonNumber = new JsonNumber()
    let private jsonString = new JsonString()

    let private parseNumber (stream: JsonStream) =
        try
            decimal (jsonNumber.Read stream)
        with
        | _ -> raise (UnexpectedJsonException());

    let private parseString (stream: JsonStream) =        
        stream.Expect quotationMark
        let value = jsonString.Read stream
        stream.Expect quotationMark

        value

    let private emptyBetween (startChar: char) (endChar: char) (stream: JsonStream) =
        let mutable empty = false
        
        stream.Skip startChar |> ignore
        stream.SkipWhitespace()

        if stream.Skip endChar then
            empty <- true

        empty

    let private emptyArray =
        fun stream -> emptyBetween beginArray endArray stream

    let private emptyObject =
        fun stream -> emptyBetween beginObject endObject stream
        
    let private parseArrayElement (stream: JsonStream) =
        stream.SkipWhitespace()
        let arrayElement = pjson stream
        stream.SkipWhitespace()

        arrayElement

    let private firstArrayElement =
        fun stream -> [parseArrayElement stream]

    let private appendArrayElement =
        fun stream array -> (parseArrayElement stream)::array

    let private parseObjectElement (stream: JsonStream) =
        stream.SkipWhitespace()
        let property = parseString stream
            
        stream.SkipWhitespace()
        stream.Expect nameSeparator
        stream.SkipWhitespace()

        let value = pjson stream
        stream.SkipWhitespace()

        (property, value)

    let private firstObjectElement =
        fun stream -> [parseObjectElement stream]

    let private appendObjectElement =
        fun stream array -> (parseObjectElement stream)::array

    let private parseArray (stream: JsonStream) =
        let mutable jsonArray = []

        if not (emptyArray stream) then
            jsonArray <- firstArrayElement stream

            while stream.Skip valueSeparator do
                jsonArray <- appendArrayElement stream jsonArray

            stream.Expect endArray

        List.rev jsonArray

    let private parseObject (stream: JsonStream) =
        let mutable jsonObject = []

        if not (emptyObject stream) then
            jsonObject <- firstObjectElement stream

            while stream.Skip valueSeparator do
                jsonObject <- appendObjectElement stream jsonObject

            stream.Expect endObject
            
        Map.ofList (List.rev jsonObject)

    [<RequireQualifiedAccess>]
    module Json =
        open System.IO

        let private jsonObject =
            fun stream -> parseObject stream |> Object

        let private jsonArray =
            fun stream -> parseArray stream |> Array

        let private jsonString =
            fun stream -> parseString stream |> String

        let private jsonNumber =
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
        /// Attempts to parse the contents of a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>The parsed JSON presented as a <see cref="Json"/> AST object.</returns>
        /// <exception cref="UnexpectedJsonException">
        /// The <paramref name="json"/> string is malformed and contains a sequence of characters that 
        /// do not adhere to the expected parsing rules, as defined by the RFC7159 specification.
        /// </exception>
        let parse json =
            use stream = new JsonStream(new StringReader(json))      
            pjson stream
