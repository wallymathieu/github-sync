module Lib.Diff

type DiffResult<'left,'right> = { 
  Right : 'right list 
  Left : 'left list
  Both : ('left * 'right) list
}

let inline diff (mapLeft:'left->'key) (left:'left list) (mapRight:'right->'key) (right:'right list) = 
  let toSetOfKeyMap map list =
    list |> List.map map |> Set.ofList
  let mapOfKeyMap map list=
    let withKey v= (map v, v)
    list |> List.map withKey |> Map.ofSeq
  let (leftSet, rightSet) = (toSetOfKeyMap mapLeft left, toSetOfKeyMap mapRight right)
  let (leftMap, rightMap) = (mapOfKeyMap mapLeft left, mapOfKeyMap mapRight right)
  let valueOfRight k=Map.find k rightMap
  let valueOfLeft k=Map.find k leftMap
  {
    Right = rightSet - leftSet |> Set.toList |> List.map valueOfRight
    Left = leftSet - rightSet |> Set.toList |>  List.map valueOfLeft 
    Both = Set.intersect rightSet leftSet |> Set.toList |>  List.map (fun key -> (valueOfLeft key, valueOfRight key))
  }
