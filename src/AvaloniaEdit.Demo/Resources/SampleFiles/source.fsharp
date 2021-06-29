let line space asterisk =
    String.replicate space " "  + String.replicate asterisk "*" + "\n"

let rec baklavaShrink =
    List.fold (fun accum n -> accum + (line n (21 - n * 2))) "" [ 1 .. 10 ]

let rec baklavaGrow =
    List.fold (fun accum n -> accum + (line n (21 - n * 2))) "" [ 10 .. -1 .. 0 ]

let baklava =
    baklavaGrow + baklavaShrink

[<EntryPoint>]
let main argv =
    printfn "%s" <| baklava.TrimEnd()
    0

