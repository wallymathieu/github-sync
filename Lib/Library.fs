namespace Lib
open FSharpPlus
open FSharpPlus.Operators
open FSharp.Control.Tasks.Builders //Ply
open Fleece.Newtonsoft
open Fleece.Newtonsoft.Operators
open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open System.Threading.Tasks
open System
open System.Text
open System.IO
module Git=
  open Fake.Core
  let clone dir sshUrl =
    try
      use stdOut = new MemoryStream()
      RawCommand( "git", Arguments.OfArgs ["clone"; sshUrl])
      |> CreateProcess.fromCommand 
      |> CreateProcess.withWorkingDirectory dir
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 2.)
      |> CreateProcess.ensureExitCode
      |> CreateProcess.withStandardOutput (UseStream (true, stdOut))
      |> Proc.run
      |> ignore
    with e -> raise <| Exception ("Failed to clone "+dir,e)
  let pull dir=
    try
      use stdOut = new MemoryStream()
      RawCommand( "git", Arguments.OfArgs ["pull"])
      |> CreateProcess.fromCommand 
      |> CreateProcess.withWorkingDirectory dir
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 2.)
      |> CreateProcess.ensureExitCode
      |> CreateProcess.withStandardOutput (UseStream (true, stdOut))
      |> Proc.run
      |> ignore
    with e -> raise <| Exception ("Failed to pull "+dir,e)

type UserType=
  |User
  |Organization
type Auth=NoAuth| Basic of string*string
  
module GitHub=
  type User={Username:string;UserType:UserType}
  let typeToString =function | User -> "users"| Organization -> "orgs"
  type Repositories= JsonProvider<"../Lib/repositories.json">
  let inline getSshUrl(r:^a) = ( ^a : ( member get_SshUrl: unit->string ) (r) )
  let inline getName(r:^a) = ( ^a : ( member get_Name: unit->string ) (r) )
  let inline getFork(r:^a) = ( ^a : ( member get_Fork: unit->bool ) (r) )
  let fetchRepos (auth:Auth) { Username=name; UserType=typ }= 
    let res=
      Http.RequestString
        ( sprintf "https://api.github.com/%s/%s/repos?per_page=300" (typeToString typ) name, 
          headers = 
            match auth with | NoAuth->[] | Basic (u,t)->[BasicAuth u t] 
            @ [ Accept HttpContentTypes.Json; UserAgent "github-sync" ] 
          )
    Repositories.Parse res
  open System.IO
  open Diff
  let syncDir (auth:Auth) (u:User) (dir:string) =
    let dirs = Directory.EnumerateDirectories(dir) |> List.ofSeq
    let repos = fetchRepos auth u |> List.ofArray |> List.filter (not << getFork)
    let directoryName d=DirectoryInfo(d).Name
    let { Right = right; Left = _;Both = i}= diff directoryName dirs getName repos
    for toClone in right do
      Git.clone dir (getSshUrl toClone)
    for (subdir,_) in i do
      if Directory.Exists <| Path.Combine(subdir, ".git") then
        Git.pull subdir
      else
        printfn "Directory does not contain .git repository"
