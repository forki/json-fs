# json-fs

[![build](https://ci.appveyor.com/api/projects/status/useeb81vu18k0irv/branch/master?svg=true)](https://ci.appveyor.com/project/ptcoda/json-fs)
[![codecov](https://codecov.io/gh/ptcoda/json-fs/branch/master/graph/badge.svg)](https://codecov.io/gh/ptcoda/json-fs)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/JsonFs.svg)](https://www.nuget.org/packages/JsonFs)

A super simple JSON library with all the functional goodness of F#. Json-Fs was created with a few goals in mind, hopefully all will be achieved.

* **Fast:** Json-Fs must be as fast, if not faster than other json libraries out there. Performance is more critical than ever before, and something as trivial as parsing json, shouldn't slow your application down. 
* **Functional Minded:** F# is a fantastic functional language. So it only seems fitting that we have a library that tries to embrace it. Step aside imperative libraries.

## Performance:

While in beta development, these metrics are likely to change often. These first set of performance metrics were captured while comparing Json-Fs against Newtonsoft (*probably the most popular json library in the world*) and Chiron (*the most functional json library I could find*).

| Parser      | Json in Bytes | No Iterations | Average Time (per msg) | Total Time      |
|-------------|--------------:|--------------:|-----------------------:|----------------:|
| Chiron      |300            |100000         |00:00:00.0000064        |00:00:02.6614482 |
| Newtonsoft  |300            |100000         |00:00:00.0000018        |00:00:00.7555658 |
| Json-Fs     |300            |100000         |00:00:00.0000012        |00:00:00.4951752 |
| Chiron      |2502           |100000         |00:00:00.0000478        |00:00:19.6419081 |
| Newtonsoft  |2502           |100000         |00:00:00.0000072        |00:00:02.9612600 |
| Json-Fs     |2502           |100000         |00:00:00.0000068        |00:00:02.8207879 |
| Chiron      |24307          |100000         |00:00:00.0004336        |00:02:58.0405911 |
| Newtonsoft  |24307          |100000         |00:00:00.0000601        |00:00:24.7080601 |
| Json-Fs     |24307          |100000         |00:00:00.0000607        |00:00:24.9267150 |