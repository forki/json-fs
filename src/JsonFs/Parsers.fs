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

    exception UnrecognisedJsonException of string

    let parseNumber (stream: CharStream) =
        try
            let number = NumberBuffer.FromStream(stream).ToString()
            Number (decimal number)
        with
        | :? System.FormatException -> raise (UnrecognisedJsonException "invalid number");

    [<RequireQualifiedAccess>]
    module Json =
        open System.IO

        let parse json =
            use stream = new CharStream(new StringReader(json))

            match stream.Peek() with
            | 't' when stream.Skip("true") -> Bool true
            | 'f' when stream.Skip("false") -> Bool false
            | 'n' when stream.Skip("null") -> Null ()
            | _ -> parseNumber stream
