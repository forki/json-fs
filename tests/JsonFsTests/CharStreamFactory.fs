module CharStreamFactory
    open ParserCs
    open System.IO

    let charStream text =
        new CharStream(new StringReader(text))

    let charStreamWithBufferSize text bufferSize =
        new CharStream(new StringReader(text), bufferSize)