#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.OpenCoverHelper
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.ReleaseNotesHelper
open System
open System.IO

// ------------------------------------------------------------------------------------------
// Build parameters

let buildDirectory = "./build/"
let reportsDirectory = "./reports/"
let toolsDirectory = "./tools"
let keysDirectory = "./keys"
let binDirectory = "./bin"

// Project files for building and testing
let sourceSets = !! "src/**/*.fsproj" ++ "src/**/*.csproj"
let testSets = !! "tests/**/*.fsproj"

// Due to PRs not supporting secure variables in AppVeyor, some steps need to be skipped
let isPullRequest = environVar "APPVEYOR_PULL_REQUEST_NUMBER" <> null

// Releases will be automatic from the master branch
let isRelease =
    let appveyorBuild = environVar "APPVEYOR" <> null

    if appveyorBuild then
        // Base branch will be master if pull request is being merged into it
        environVar "APPVEYOR_REPO_BRANCH" = "master" && not isPullRequest
    else
        getBranchName __SOURCE_DIRECTORY__ = "master"

if isPullRequest then trace "We are about to build a pull request"
if isRelease then trace "We are about to build a release"

// Extract information from the pending release
let releaseNotes = parseReleaseNotes (File.ReadAllLines "RELEASE_NOTES.md")

// Not long till Windows 10 supports Bash natively :)
setEnvironVar "PATH" "C:\\cygwin64;C:\\cygwin64\\bin;C:\\cygwin;C:\\cygwin\\bin;%PATH%"

// ------------------------------------------------------------------------------------------
// Clean targets

Target "Clean" (fun _ -> 
    CleanDirs[buildDirectory; reportsDirectory; toolsDirectory; binDirectory]
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

    let getAssemblyInfoAttributes projectName =
        let assemblyFileVersion = 
            let buildNumber = environVarOrDefault "APPVEYOR_BUILD_NUMBER" "0"
            sprintf "%s.%s" releaseNotes.AssemblyVersion buildNumber

        let signedInternalsVisibleTo = 
            let publicKey = (File.ReadAllText "./keys/JsonFs.pk")
            sprintf "JsonFsTests,PublicKey=%s" publicKey

        let attributes = [ 
          Attribute.Title (projectName)
          Attribute.Product "JsonFs"
          Attribute.Description "A super simple JSON library with all the functional goodness of F#"
          Attribute.Company "Coda Solutions Ltd"
          Attribute.Version "0.1.0"
          Attribute.FileVersion assemblyFileVersion ]

        if isPullRequest then
            List.concat [
                attributes; 
                [ Attribute.InternalsVisibleTo "JsonFsTests" ]]
        else
            List.concat [
                attributes; 
                [ Attribute.KeyFile "../../keys/JsonFs.snk"; 
                  Attribute.InternalsVisibleTo signedInternalsVisibleTo ]]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        let projectDirectory = System.IO.Path.GetDirectoryName(projectPath)

        ( projectPath,
          projectName,
          projectDirectory,
          System.IO.File.Exists(projectDirectory @@ "AssemblyInfo.fs"),
          (getAssemblyInfoAttributes projectName)
        )

    // TODO: check if projFileName and projectName are even used... else remove

    sourceSets
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, isFSharpProject, attributes) ->
        match isFSharpProject with
        | true -> CreateFSharpAssemblyInfo (folderName @@ "AssemblyInfo.fs") attributes
        | false -> CreateCSharpAssemblyInfo (folderName @@ "Properties/AssemblyInfo.cs") attributes)
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
            OptionalArguments = "-excludebyfile:*\*AssemblyInfo.fs;*\*AssemblyInfo.cs;*\*Ast.fs -hideskipped:File"
            Filter = "+[JsonFs*]* +[JsonCs*]* -[*Tests]* -[*Performance]*"
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

// ------------------------------------------------------------------------------------------
// Generate Nuget package and deploy

Target "NugetPackage" (fun _ ->
    trace "Building Nuget package with Paket..."

    Paket.Pack (fun p -> 
        { p with
            OutputPath = binDirectory
            Symbols = true
            Version = releaseNotes.NugetVersion
            ReleaseNotes = releaseNotes.Notes |> String.concat Environment.NewLine
        })
)

Target "PublishNugetPackage" (fun _ ->
    trace "Publishing Nuget package with Paket..."

    Paket.Push (fun p ->
        { p with
            WorkingDir = binDirectory
        })
)

Target "All" DoNothing

"Clean"
    =?> ("DecryptSigningKey", not isPullRequest)
    ==> "PatchAssemblyInfo"
    ==> "Build"
    ==> "BuildTests"
    ==> "RunUnitTests"
    ==> "PublishCodeCoverage"
    =?> ("NugetPackage", isRelease)
    =?> ("PublishNugetPackage", isRelease)
    ==> "All"

RunTargetOrDefault "All"