namespace Forma.StaticForms.Javascript

module ValueMap =

    open Forma.Forms
    open Forma.StaticForms
    open ToolBox.Core.Strings

    let isDisabled =
        [ "export const isDisabled = (element) => {"
          "    if (element.dataset.disabled == 1) {"
          "        return true;"
          "    }"
          "    else {"
          "        return false;"
          "    }"
          "}"
          "" ]
        |> concat

    let buildObject =
        [ "export const buildObject = (id, name, obj) => {"
          "    const el = document.getElementById(id);"
          "    if (isDisabled(el)) {"
          "        return obj;"
          "    }"
          "    else {"
          "        obj[name] = el.value;"
          "        return obj;"
          "    }"
          "}"
          "" ]
        |> concat
    //|> concat

    let renderValueMaps (form: Form) =

        let map =
            match form.Body with
            | FormBody.Pages pages -> pages |> List.collect (fun p -> p.Elements)
            | FormBody.Elements elements -> elements
            |> List.collect getFields
            |> List.map (fun f -> $"['{f.Id}', '{f.Id.ToCamelCase()}']")
            |> List.reduce (fun a b -> $"{a}, {b}")

        [ "export const createJson = () => {"
          "    let obj ="
          "        ["
          $"            {map}"
          "        ].reduce((prev, [id, name]) => buildObject(id, name, prev), {});"
          ""
          "    return obj;"
          "}" ]
        |> concat


    let render (outputPath: string) (form: Form) =

        [ isDisabled
          buildObject
          renderValueMaps form ]
        |> concat
        |> writeToFile outputPath "value_map.js"