namespace Forma.StaticForms.Javascript


module Validation =

    open System
    open System.IO
    open ToolBox.Core.Strings
    open Forma.Forms
    open Forma.StaticForms

    let idToName (id: string) = id.ToCamelCase()

    let indent str = $"    {str}"

    let makeFnName (field: Field) (i: int) =
        $"{field.Id.ToCamelCase()}Validation{i + 1}"

    let generateFieldValidation (field: Field) =

        let (validationJsFn, reduces) =
            field.Validation
            |> List.mapi (fun i vt ->
                let fnName = makeFnName field i

                match vt with
                | ValidationType.NotBlank message ->
                    [ $"export const {fnName} = (message, validationObj, element) => {{"
                      ""
                      "    if (element.value == '') {"
                      "        validationObj.messages.push(message);"
                      "        validationObj.valid = false;"
                      "    }"
                      ""
                      "    return validationObj;"
                      "}"
                      "" ],
                    $"[{fnName}, '{message}']"
                | ValidationType.RegexMatch (pattern, caseInsensitive, message) ->
                    let caseCheck =
                        if caseInsensitive then
                            ".toLowerCase()"
                        else
                            ""

                    [ $"export const {fnName} = (message, validationObj, element) => {{"
                      $"    const regexp = {pattern};"
                      $"    if (regexp.test(String(element.value){caseCheck}) == false) {{"
                      "        validationObj.messages.push(message);"
                      "        validationObj.valid = false;"
                      "    }"
                      ""
                      "    return validationObj;"
                      "}"
                      "" ],
                    $"[{fnName}, '{message}']"
                | ValidationType.StringMatch (str, caseInsensitive, message) ->
                    let caseCheck =
                        if caseInsensitive then
                            ".toLowerCase()"
                        else
                            ""

                    [ $"export const {fnName} = (message, validationObj, element) => {{"
                      $"    if (String(element.value){caseCheck} != String('{str}'){caseCheck}) {{"
                      "        validationObj.messages.push();"
                      "        validationObj.valid = false;"
                      "    }"
                      "    return validationObj;"
                      "}"
                      "" ],
                    $"[{fnName}, '{message}']")
            |> List.fold (fun (jsFns, reduces) (jsFn, reduce) -> jsFns @ jsFn, reduces @ [ reduce ]) ([], [])


        let reduce =
            reduces |> List.reduce (fun a b -> $"{a},{b}")


        [ yield! validationJsFn
          $"export const {field.Id.ToCamelCase()}Validation = () => {{"
          $"    const field = document.getElementById('{field.Id}');"
          $"    const fieldMessage = document.getElementById('{field.Id}-validation-message');"
          "    if (isDisabled(field)) return true;"
          ""
          "    let result ="
          "        ["
          $"            {reduce}"
          "        ].reduce("
          "            (prev, [curr, message]) => curr(message, prev, field),"
          "            { valid: true, messages: [] }"
          "        );"
          ""
          "    if (result.valid == false) {"
          "        fieldMessage.innerHTML = result.messages.reduce((prev, curr) => prev + '<br>' + curr, '');"
          "        fieldMessage.classList.add('show');"
          "        field.classList.add('invalid-input');"
          "    }"
          ""
          "    else {"
          "        fieldMessage.innerText = '';"
          "        field.classList.remove('show');"
          "        field.classList.remove('invalid-input');"
          "    }"
          ""
          "    return result.valid;" |> indent
          "}"
          ""
          $"export const {field.Id.ToCamelCase()}RecheckValidation = () => {{"
          $"    const field = document.getElementById('{field.Id}');"
          $"    const fieldMessage = document.getElementById('{field.Id}-validation-message');"
          "    if (fieldMessage.classList.contains('show')) {"
          $"        {field.Id.ToCamelCase()}Validation();"
          "    }"
          "}"
          "" ]
        |> String.concat Environment.NewLine


    let generatePageValidation (page: FormPage) =

        let (fns, cases, checks) =
            page.Elements
            |> List.map getFields
            |> List.concat
            |> List.choose (fun f ->
                match f.Validation.IsEmpty with
                | true -> None
                | false ->
                    let case =
                        [ $"case '{f.Id}':"
                          $"    return {f.Id.ToCamelCase()}RecheckValidation();" ]

                    Some(generateFieldValidation f, case, $"{f.Id.ToCamelCase()}Validation()"))
            |> List.fold
                (fun (fns, cases, checks) (fn, case, check) -> fns @ [ fn ], cases @ case, checks @ [ check ])
                ([], [], [])


        let pageValidation =
            [ $"export const {page.Id.ToCamelCase()}Validation = () => {{"
              "    const result ="
              "        ["
              checks
              |> List.map (fun s -> indent s |> indent |> indent)
              |> List.reduce (fun a b -> $"{a},{Environment.NewLine}{b}")
              "        ].filter(x => x == false)"
              "    return result.length == 0;"
              "}"
              "" ]


        fns |> String.concat Environment.NewLine,
        pageValidation
        |> String.concat Environment.NewLine,
        cases


    let generatePagesValidations (pages: FormPage list) =
        let fns, pv, rechecks =
            pages
            |> List.map generatePageValidation
            |> List.fold
                (fun (fns, pvs, rechecks) (fn, pv, recheck) -> fns @ [ fn ], pvs @ [ pv ], rechecks @ recheck)
                ([], [], [])

        let cases =
            pages
            |> List.map (fun p ->
                [ $"case '{p.Id}':"
                  $"     return {p.Id.ToCamelCase()}Validation();" ])

        [ "export const isDisabled = (element) => {"
          "    if (element.dataset.disabled == 1) {"
          "        return true;"
          "    }"
          "    else {"
          "        return false;"
          "    }"
          "}"
          ""
          yield! fns
          yield! pv
          "export const validatePage = (page) => {"
          "    switch (page) {"
          yield!
              cases
              |> List.concat
              |> List.map (fun c -> indent c |> indent)
          "        default:"
          "            return true;"
          "    }"
          "}"
          ""
          "export const recheck = (id) => {"
          "    switch(id) {"
          yield! rechecks |> List.map (fun c -> indent c |> indent)
          "        default:"
          "            return;"
          "    }"
          "}" ]
        |> String.concat Environment.NewLine

    let render (outputPath: string) (pages: FormPage list) =
        File.WriteAllText(Path.Combine(outputPath, "validation.js"), generatePagesValidations pages)
