namespace Forma.StaticForms.Html


module HtmlRenderer =

    open Forma.Forms
    open Forma.StaticForms
    open Forma.StaticForms.Html.Form

    let run (outputPath: string) (form: Form) =

        renderForm form
        |> concat
        |> writeToFile outputPath "form.html"
