namespace Forma.StaticForms

[<RequireQualifiedAccess>]
module StaticFormsRenderer =

    open System.IO
    open Forma.Forms
    open Forma.StaticForms.Javascript
    open Forma.StaticForms.Html
    
    let run (outputPath: string) (form: Form) =
        
        let jsPath = Path.Combine(outputPath, "js")
        
        if Directory.Exists jsPath |> not then Directory.CreateDirectory jsPath |> ignore
        
        JavascriptRenderer.run jsPath form
        
        HtmlRenderer.run outputPath form
        
        ()

