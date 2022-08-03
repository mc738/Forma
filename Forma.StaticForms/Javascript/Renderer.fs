namespace Forma.StaticForms.Javascript

[<RequireQualifiedAccess>]
module JavascriptRenderer =
    
    open Forma.Forms
    open Forma.StaticForms
    
    let staticFormsJs =
        [ "import * as validation from './validation.js';"
          "import * as conditions from './conditions.js';"
          "import * as values from './value_map.js';"
          ""
          "const pages = document.getElementsByClassName('card-page');"
          "const progresses = document.getElementsByClassName('progress-item');"
          ""
          "window.recheck = function(evt) {"
          "    validation.recheck(evt.target.id);"
          "    conditions.test();"
          "}"
          ""
          "window.changePage = function(curr, next, validate) {"
          "    if ((validate && validation.validatePage(curr)) || (!validate)) {"
          ""
          "        Array.from(progresses).forEach(element => { element.classList.remove('current') });"
          ""
          "        Array.from(pages).forEach(element => {"
          "            if (element.id === next) {"
          "                element.classList.add('show');"
          "                const progress = document.getElementById(`${element.id}-progress`);"
          "                progress.classList.add('current');"
          "            }"
          "            else {"
          "                element.classList.remove('show');"
          "            }"
          "        })"
          "    }"
          "}"
          ""
          "window.initialize = function() {"
          "    conditions.initialize();"
          "}"
          ""
          "window.submit = function() {"
          "    console.log(values.createJson());"
          "}" ]
        |> concat

    let run (outputPath: string) (form: Form) =
        
        let pages =
            match form.Body with
            | FormBody.Pages fp -> fp
            | _ -> []
        
        pages |> Validation.render outputPath

        pages |> Conditions.render outputPath
        
        form |> ValueMap.render outputPath
        
        writeToFile outputPath $"static_form.js" staticFormsJs
        
    
    

