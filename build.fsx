#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.OpenCoverHelper
open Fake.AssemblyInfoFile
open System
open System.IO

// ------------------------------------------------------------------------------------------
// Build parameters

let buildDirectory = "./build/"
let reportsDirectory = "./reports/"
let toolsDirectory = "./tools"
let keysDirectory = "./keys"

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

Target "DecryptSigningKey" (fun _ ->
    trace "Decrypt signing key for strong named assemblies..."

    Copy keysDirectory ["./packages/build/secure-file/tools/secure-file.exe"]

    let decryptKeyPath = currentDirectory @@ "keys" @@ "decrypt-key.cmd"

    let exitCode = ExecProcess (fun info -> 
        info.FileName <- decryptKeyPath
        info.WorkingDirectory <- keysDirectory) (TimeSpan.FromMinutes 1.0)

    if exitCode <> 0 then
        failwithf "Failed to decrypt the signing key"
)

Target "PatchAssemblyInfo" (fun _ ->
    trace "Patching all assemblies..."

    let publicKey = (File.ReadAllText "./keys/JsonFs.pk")
    let buildNumber = environVarOrDefault "APPVEYOR_BUILD_NUMBER" "0"
    let assemblyFileVersion = (sprintf "0.1.0.%s" buildNumber)

    trace (sprintf "With assembly file version: %s" assemblyFileVersion)

    let getAssemblyInfoAttributes projectName =
        [ Attribute.Title (projectName)
          Attribute.Product "JsonFs"
          Attribute.Description "A super simple JSON library with all the functional goodness of F#"
          Attribute.Company "Coda Solutions Ltd"
          Attribute.Version "0.1.0"
          Attribute.FileVersion assemblyFileVersion
          Attribute.KeyFile "../../keys/JsonFs.snk"
          Attribute.InternalsVisibleTo (sprintf "JsonFsTests,PublicKey=%s" publicKey) ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    sourceSets
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, attributes) ->
        CreateFSharpAssemblyInfo (folderName @@ "AssemblyInfo.fs") attributes)
)

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
            Filter = "+[JsonFs*]* -[*Tests]*"
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
    ==> "DecryptSigningKey"
    ==> "PatchAssemblyInfo"
    ==> "Build"
    ==> "BuildTests"
    ==> "RunUnitTests"
    ==> "PublishCodeCoverage"
    ==> "All"

RunTargetOrDefault "All"