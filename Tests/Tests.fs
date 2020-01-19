namespace Facts

open System
open Xunit
open FsUnit.Xunit
open FsUnit
open Lib.Diff

module ``Given an existing list with one element`` =
  let existing = [1]
    
  let inline id x = x
  let diffsame left right = diff id left id right
  [<Fact>] 
  let ``Right contains one an extra element.`` ()=
    diffsame [1] [2;1] |> should equal { Right = [2]; Left = []; Both = [(1,1)]}

  [<Fact>] 
  let ``Left contains an element`` ()=
    let expected:DiffResult<int,int>= { Right = []; Left = [1]; Both = []}
    diffsame [1] [] |> should equal expected
  [<Fact>] 
  let ``Right contains an element`` ()=
    let expected:DiffResult<int,int> = { Right = [1]; Left = []; Both = []}
    diffsame [] [1] |> should equal expected

  [<Fact>] 
  let ``Both sides have the same elements`` ()=
      diff id existing id existing |> should equal { Right = []; Left = []; Both = [(1,1)]}
