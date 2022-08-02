namespace Forma.StaticForms.Javascript

module Conditions =

    open System
    open System.IO
    open Forma.Forms
    open ToolBox.Core.Strings

    let initializeJs =
        [ "export const initialize = () => {"
          "    Array.from(document.querySelectorAll('[data-element]')).forEach(x => {"
          "        if (x.dataset.disabled  == 1) {"
          "            document.getElementById(`${x.id}-container`).style.display = 'none';"
          "        }"
          "    })"
          ""
          "    test();"
          "}"
          "" ]
        |> String.concat Environment.NewLine

    let testBranchJs =
        [ "export const testBranch = (branchId) => {"
          "    let branchElements = Array.from(document.querySelectorAll(`[data-branch='${branchId}']`));"
          ""
          "    if (branchTests(branchId)) {"
          "        branchElements.forEach(x => {"
          "            x.dataset.disabled = 0;"
          "            document.getElementById(`${x.id}-container`).style.display = 'block';"
          "        })"
          "    }"
          "    else {"
          "        branchElements.forEach(x => {"
          "            x.dataset.disabled = 1;"
          "            document.getElementById(`${x.id}-container`).style.display = 'none';"
          "        })"
          "    }"
          "}"
          "" ]
        |> String.concat Environment.NewLine

    let rec generateCondition (condition: Condition) (level: int) (i: int) =
        let cs b = if b then ".toLowerCase()" else ""

        match condition with
        | Condition.NotBlank id -> $"    let l{level}c{i} = document.getElementById('{id}').value !== '';"
        | Condition.RegexMatch rm ->
            $"    let l{level}c{i} = {rm.Pattern}.test(String(document.getElementById({rm.Id}).value){cs rm.CaseInsensitive});"
        | Condition.StringMatch sm ->
            $"    let l{level}c{i} = String(document.getElementById({sm.Id}).value){cs sm.CaseInsensitive} === '{sm.Value}';"
        | Condition.All conditions ->
            // Create all conditions
            let subCons, checks =
                conditions
                |> List.mapi (fun i c -> generateCondition c (level + 1) (i + 1), $"l{level + 1}c{i + 1}")
                |> List.fold (fun (acc1, acc2) (a, b) -> acc1 @ [ a ], acc2 @ [ b ]) ([], []) //|> String.concat Environment.NewLine
                |> fun (sc, c) -> sc, c |> List.reduce (fun a b -> $"{a} && {b}")

            [ yield! subCons
              $"    let l{level}c{i} ="
              $"        {checks};" ]
            |> String.concat Environment.NewLine
        | Condition.Any conditions ->
            // Create all conditions
            let subCons, checks =
                conditions
                |> List.mapi (fun i c -> generateCondition c (level + 1) (i + 1), $"l{level + 1}c{i + 1}")
                |> List.fold (fun (acc1, acc2) (a, b) -> acc1 @ [ a ], acc2 @ [ b ]) ([], []) //|> String.concat Environment.NewLine
                |> fun (sc, c) -> sc, c |> List.reduce (fun a b -> $"{a} || {b}")

            [ yield! subCons
              $"    let l{level}c{i} ="
              $"        {checks};" ]
            |> String.concat Environment.NewLine
        
    let generateBranchTest (branch: Branch) =

        [ $"export const {branch.Id.ToCamelCase()}Condition = () => {{"
          generateCondition branch.Condition 1 1
          |> fun r ->
            [ r; ""; $"    return l{1}c{1};" ]
            |> String.concat Environment.NewLine
          "}" ]
        |> String.concat Environment.NewLine

    let generateConditions (elements: Element list) =
        let branches =
            elements
            |> List.choose (fun el ->
                match el with
                | Branch b -> Some b
                | _ -> None)

        let testFn =
            branches
            |> List.map (fun b -> $"    testBranch('{b.Id}')")
            |> fun r ->
                [ "export const test = () => {"
                  yield! r
                  "}"
                  "" ]
                |> String.concat Environment.NewLine

        let branchTestsFn =
            branches
            |> List.collect (fun b ->
                [ $"        case '{b.Id}':"
                  $"            return {b.Id.ToCamelCase()}Condition();" ])
            |> fun r ->
                [ "export const branchTests = (branchId) => {"
                  "    switch (branchId) {"
                  yield! r
                  "        default:"
                  "            return false;"
                  "    }"
                  "}"
                  "" ]
                |> String.concat Environment.NewLine

        [ initializeJs
          yield! branches |> List.map generateBranchTest
          branchTestsFn
          testBranchJs
          testFn ]
        |> String.concat Environment.NewLine

    let render (outputPath: string) (pages: FormPage list) =
        
        File.WriteAllText(Path.Combine(outputPath, "conditions.js"), generateConditions (pages |> List.collect (fun p -> p.Elements)))


