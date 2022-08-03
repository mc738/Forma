namespace Forma.StaticForms.Html


module Form =

    open System
    open Forma.Forms
    open Forma.StaticForms
        
    let indent i str = $"{String(' ', i * 4)}{str}"

    let renderInput (field: Field) (branchName: string option) =
        let bna =
            match branchName with
            | Some bn -> $" data-branch=\"{bn}\""
            | None -> ""

        match field.Type with
        | FieldType.Text t -> $"<input id=\"{field.Id}\" oninput=\"recheck(event)\" data-element=\"1\"{bna} data-disabled=\"0\">"
        | FieldType.TextArea ->
            $"<textarea id=\"{field.Id}\" oninput=\"recheck(event)\" data-element=\"1\"{bna} data-disabled=\"0\"></textarea>"
        | FieldType.Selector _ -> failwith "Selector not implemented yet."

    let rec generateElements (branchId: string option) (elements: Element list) =
        elements
        |> List.collect (fun element ->
            match element with
            | Element.Field f ->
                [ $"<div id=\"{f.Id}-container\" class=\"input-container\">"
                  $"    <label for=\"{f.Id}\">{f.Label}</label>"
                  match f.Subtitle with
                  | Some st -> $"    <p class=\"subtitle\">{st}</p>"
                  | None -> ()
                  renderInput f branchId |> indent 1
                  $"    <p id=\"{f.Id}-validation-message\" class=\"validation-message\"></p>"
                  "</div>" ]

            | Element.Branch b ->
                [ $"<div class=\"{b.Id}\">"
                  yield!
                      generateElements (Some b.Id) b.Elements
                      |> List.map (indent 1)
                  "</div>" ])
    //|> List.map (indent 4)

    let renderControls (prevPage: FormPage option) (nextPage: FormPage option) (page: FormPage) =

        [ "<div class=\"form-controls\">"
          "    <div class=\"control-container\">"
          yield!
              match prevPage with
              | Some pp ->
                  [ $"        <button class=\"control-button\" onclick=\"changePage('{page.Id}', '{pp.Id}', false)\">"
                    "            <p>< Prev</p>"
                    "        </button>" ]
              | None -> [ "" ]
          "    </div>"
          "    <div class=\"control-container right\">"
          yield!
              match nextPage with
              | Some np ->

                  [ $"        <button class=\"control-button\" onclick=\"changePage('{page.Id}', '{np.Id}', true)\">"
                    "            <p>Next ></p>"
                    "        </button>" ]
              | None ->
                  [ $"        <button class=\"control-button\" onclick=\"submit()\">"
                    "            <p>Submit</p>"
                    "        </button>" ]
          "    </div>"
          "</div>" ]

    let generatePage (prev: FormPage option) (next: FormPage option) (page: FormPage) =

        let pageClasses =
            if prev.IsNone then
                "card-page show"
            else
                "card-page"

        [ $"<div id=\"{page.Id}\" class=\"{pageClasses}\">"
          page.Elements
          |> generateElements None
          |> List.map (indent 1)
          |> concat
          page
          |> renderControls prev next
          |> List.map (indent 1)
          |> concat
          "</div>" ]

    let renderProgress (pages: FormPage list) =
        pages
        |> List.mapi (fun i p ->
            let classes =
                if i = 0 then
                    "progress-item current"
                else
                    "progress-item"

            [ $"<div id=\"page-{i + 1}-progress\" class=\"{classes}\">"
              $"    <p><span>{i + 1}</span>{p.Title}</p>"
              "</div>" ])
        |> List.concat
        |> fun r ->
            [ "<div class=\"form-progress\">"
              r |> List.map (indent 1) |> concat
              "</div>" ]

    let renderForm (form: Form) =
        match form.Body with
        | FormBody.Pages formPages ->

            let getPrev i = formPages |> List.tryItem (i - 1)
            let getNext i = formPages |> List.tryItem (i + 1)

            [ "<div class=\"card-body\">"
              formPages
              |> renderProgress
              |> List.map (indent 1)
              |> concat
              formPages
              |> List.mapi (fun i -> generatePage (getPrev i) (getNext i))
              |> List.concat
              |> List.map (indent 1)
              |> concat
              "</div>" ]
        | FormBody.Elements _ -> failwith $"TODO: Top level elements to implement."
        |> fun r ->
            [ "<div class=\"form form-card\">"
              "    <div class=\"card-title\">"
              $"        <h1>{form.Name}</h1>"
              "    </div>"
              r |> List.map (indent 1) |> concat
              "</div>" ]


