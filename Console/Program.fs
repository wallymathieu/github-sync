// Learn more about F# at http://fsharp.org
open FSharpPlus
open System.Linq
open System.IO
open System
open Lib
open Lib.GitHub

[<Diagnostics.CodeAnalysis.SuppressMessage("*", "EnumCasesNames")>]
type Cmd=
  |fetch=0
type CmdArgs =
  { Command: Cmd option; Dir: string; GitHubUser:GitHub.User option }
let (|Cmd|_|) : _-> Cmd option = tryParse
[<EntryPoint>]
let main argv =
  let defaultArgs = { Command = None; Dir = Directory.GetCurrentDirectory() ; GitHubUser=None}
  let usage =
   ["Usage:"
    sprintf "    --dir     DIRECTORY  where to store data (Default: %s)" defaultArgs.Dir
    sprintf "    --user    GitHub user"
    sprintf "    --org     GitHub org"
    sprintf "    COMMAND    one of [%s]" (Enum.GetValues( typeof<Cmd> ).Cast<Cmd>() |> Seq.map string |> String.concat ", " )
    sprintf "   Auth : Use github_api_username and github_api_token environment name"]
    |> String.concat Environment.NewLine
  let rec parseArgs b args =
    match args with
    | [] -> Ok b
    | "--dir" :: dir :: xs -> parseArgs { b with Dir = dir } xs
    | "--org" :: username :: xs -> parseArgs { b with GitHubUser = Some { Username = username; UserType=Organization } } xs
    | "--user" :: username :: xs -> parseArgs { b with GitHubUser = Some { Username = username; UserType=User } } xs
    | Cmd cmd :: xs-> parseArgs { b with Command = Some cmd } xs
    | invalidArgs ->
      sprintf "error: invalid arguments %A" invalidArgs |> Error

  match argv |> List.ofArray |> parseArgs defaultArgs with
  | Ok args->
    match args with
    | { Dir=dir; Command=Some command; GitHubUser=user } ->
      match command, user with
      | Cmd.fetch, Some user ->
        let github_api_username = Environment.GetEnvironmentVariable("github_api_username")
        let github_api_token = Environment.GetEnvironmentVariable("github_api_token")
        let auth = if String.IsNullOrEmpty( github_api_token) && String.IsNullOrEmpty(github_api_username) then Basic(github_api_username,github_api_token) else NoAuth
        GitHub.syncDir auth user dir
        0
      | (Cmd.fetch, None) ->
        printfn "error: Expected user"
        printfn "%s" usage
        1
      | (_, _) ->
        printfn "error: Expected command"
        printfn "%s" usage
        1
    | _ ->
      printfn "error: Expected command"
      printfn "%s" usage
      1
  | Error err->
      printfn "%s" err
      printfn "%s" usage
      1
