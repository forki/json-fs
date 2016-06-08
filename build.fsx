#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.OpenCoverHelper
open System

// ------------------------------------------------------------------------------------------
// Build parameters

let buildDirectory = "./build/"
let reportsDirectory = "./reports/"
let toolsDirectory = "./tools"

// Project files for building and testing
let sourceSets = !! "src/**/*.fsproj"
let testSets = !! "tests/**/*.fsproj"

// Not long till Windows 10 supports Bash natively :)

setEnvironVar "PATH" "C:\\cygwin64;C:\\cygwin64\\bin;C:\\cygwin;C:\\cygwin\\bin;%PATH%"

// ------------------------------------------------------------------------------------------
// Clean targets

Target "Clean" (fun _ -> 
    CleanDirs[buildDirectory; reportsDirectory; toolsDirectory]
)

// ------------------------------------------------------------------------------------------
// Build source and test projects

Target "Build" (fun _ ->
    MSBuildRelease buildDirectory "Rebuild" sourceSets
    |> Log "Building: "
)

Target "BuildTests" (fun _ ->
    MSBuildDebug buildDirectory "Build" testSets
    |> Log "Building: "
)

// ------------------------------------------------------------------------------------------
// Run tests and gather code coverage

let codeCoverageReport = (reportsDirectory @@ "code-coverage.xml")

Target "RunUnitTests" (fun _ ->
    trace "Executing tests and generating code coverage with OpenCover..."

    let assembliesToTest = 
        !! (buildDirectory @@ "*Tests.dll") 
        |> Seq.toArray 
        |> String.concat " "

    OpenCover (fun p ->
        { p with
            ExePath = "./packages/build/OpenCover/tools/OpenCover.Console.exe"
            WorkingDir = __SOURCE_DIRECTORY__
            TestRunnerExePath = "./packages/build/xunit.runner.console/tools/xunit.console.exe"
            Output = codeCoverageReport
            Register = RegisterType.RegisterUser
            Filter = "+[JsonFs*]* -[*.Tests*]*"
        })
        (assembliesToTest + " -appveyor -noshadow")
)

Target "PublishCodeCoverage" (fun _ ->
    trace "Publishing code coverage report to CodeCov..."

    let codeCovScript = (toolsDirectory @@ "CodeCov.sh")

    let exitCode = ExecProcess (fun info ->
        info.FileName <- "curl"
        info.Arguments <- "-s https://codecov.io/bash -o " + codeCovScript) (TimeSpan.FromMinutes 2.0)

    if exitCode <> 0 then
        failwithf "Could not download the bash uploader from CodeCov.io. Expecting cygwin and curl to be installed"

    let exitCode = ExecProcess (fun info -> 
        info.FileName <- "bash"
        info.Arguments <- (sprintf "%s -f %s" codeCovScript codeCoverageReport)) (TimeSpan.FromMinutes 5.0)

    if exitCode <> 0 then
        failwithf "Failed to upload the codecov coverage report"
)

Target "All" DoNothing

"Clean"
    ==> "Build"
    ==> "BuildTests"
    ==> "RunUnitTests"
    ==> "PublishCodeCoverage"
    ==> "All"

RunTargetOrDefault "All"