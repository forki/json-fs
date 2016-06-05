#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake

Target "All" DoNothing

RunTargetOrDefault "All"