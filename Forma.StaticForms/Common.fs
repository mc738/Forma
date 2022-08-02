namespace Forma.StaticForms

[<AutoOpen>]
module Common =
    open System
    open System.IO

    let concat (vs: string list) = vs |> String.concat Environment.NewLine
    
    let writeToFile (outputPath: string) (name: string) data =
        File.WriteAllText(Path.Combine(outputPath, name), data)