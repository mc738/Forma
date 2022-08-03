namespace Forma.StaticForms

[<AutoOpen>]
module Common =
    open System
    open System.IO
    open Forma.Forms

    let concat (vs: string list) = vs |> String.concat Environment.NewLine
    
    let writeToFile (outputPath: string) (name: string) data =
        File.WriteAllText(Path.Combine(outputPath, name), data)
        
    let rec getFields (element: Element) =
            match element with
            | Field f -> [ f ]
            | Branch b -> b.Elements |> List.collect getFields