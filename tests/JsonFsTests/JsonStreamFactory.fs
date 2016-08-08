module JsonStreamFactory
    open ParserCs
    open System.IO

    let jsonStream text =
        new JsonStream(new StringReader(text))

    let jsonStreamWithBufferSize text bufferSize =
        new JsonStream(new StringReader(text), bufferSize)