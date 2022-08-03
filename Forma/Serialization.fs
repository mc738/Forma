namespace Forma

open System.Xml
open Forma.Forms
open Microsoft.FSharp.Core

module Serialization =

    open System
    open System.Text.Json
    open ToolBox.Core
    open Forma.Forms

    let equals strA strB =
        String.Equals(strA, strB, StringComparison.OrdinalIgnoreCase)

    let (?=) a b = equals a b

    let toMap (properties: JsonProperty list) =
        properties
        |> List.map (fun p -> p.Name, p.Value)
        |> Map.ofList


    let getString key el = Json.tryGetStringProperty key el

    let getType el = getString "type" el

    let getId el = getString "id" el

    let substituteValue (values: Map<string, string>) (value: string option) =
        value
        |> Option.bind (fun v ->
            match v.StartsWith('@') with
            | true -> values.TryFind v
            | false -> Some v)

    let collectResults<'T> (results: Result<'T, string> list) =
        results
        |> List.fold
            (fun (successes, errors) r ->
                match r with
                | Ok s -> successes @ [ s ], errors
                | Error e -> successes, errors @ [ e ])
            ([], [])
        |> fun (successes, errors) ->
            match errors.IsEmpty with
            | true -> Ok successes
            | false -> errors |> String.concat " " |> Error

    let deserializeValidation (element: JsonElement) =
        match getType element, Json.tryGetStringProperty "message" element with
        | Some t, Some m when t ?= "notblank" -> Ok(ValidationType.NotBlank m)
        | Some t, Some m when t ?= "regex" ->
            match Json.tryGetStringProperty "pattern" element
                  |> substituteValue Validation.wellKnownRegexes,
                  Json.tryGetBoolProperty "caseInsensitive" element
                with
            | Some p, Some cs -> Ok(ValidationType.RegexMatch(p, cs, m))
            | None, _ -> Error "Missing `pattern` property."
            | _, None -> Error "Missing `caseInsensitive` property."
        | Some t, Some m when t ?= "string" ->
            match Json.tryGetStringProperty "value" element, Json.tryGetBoolProperty "caseInsensitive" element with
            | Some v, Some cs -> Ok(ValidationType.StringMatch(v, cs, m))
            | None, _ -> Error "Missing `value` property."
            | _, None -> Error "Missing `caseInsensitive` property."
        | Some t, _ -> Error $"Unknown validation type `{t}`."
        | None, _ -> Error "Missing `type` property on validation."
        | _, None -> Error "Missing `message` property on validation."

    let deserializeFieldType (obj: JsonElement) =
        match Json.tryGetStringProperty "type" obj with
        | Some t when t |> equals "text" ->
            match Json.tryGetStringProperty "textInput" obj with
            | Some v when v |> equals "decimal" -> Ok(FieldType.Text TextInput.Decimal)
            | Some v when v |> equals "email" -> Ok(FieldType.Text TextInput.Email)
            | Some v when v |> equals "integer" -> Ok(FieldType.Text TextInput.Integer)
            | Some v when v |> equals "phone" -> Ok(FieldType.Text TextInput.Phone)
            | Some v when v |> equals "text" -> Ok(FieldType.Text TextInput.Text)
            | Some v -> Error $"Unknown text input type `{v}`."
            | None -> Error "Missing textInput property."
        | Some t when t |> equals "textarea" -> Ok <| FieldType.TextArea
        | Some t -> Error $"Unknown field type `{t}`."
        | None -> Error "Missing/invalid type property."

    let deserializeField (element: JsonElement) =

        match Json.tryGetStringProperty "id" element,
              Json.tryGetStringProperty "label" element,
              Json.tryGetProperty "fieldType" element,
              Json.tryGetArrayProperty "validation" element
            with
        | Some id, Some label, Some typeObj, Some validationArr ->
            deserializeFieldType typeObj
            |> Result.bind (fun tf ->
                validationArr
                |> List.map deserializeValidation
                |> collectResults
                |> Result.map (fun v -> tf, v))
            |> Result.map (fun (tf, v) ->
                ({ Id = id
                   Label = label
                   Subtitle = Json.tryGetStringProperty "subtitle" element
                   Type = tf
                   Validation = v }: Field))
        | None, _, _, _ -> Error "Missing `id` property."
        | _, None, _, _ -> Error "Missing `label` property."
        | _, _, None, _ -> Error "Missing `fieldType` property."
        | _, _, _, None -> Error "Missing `validation` property."

    let rec deserializeCondition (element: JsonElement) =
        match getType element with
        | Some t when t ?= "all" ->
            match Json.tryGetArrayProperty "conditions" element with
            | Some cs ->
                cs
                |> List.map deserializeCondition
                |> collectResults
                |> Result.map Condition.All
            | None -> Error "Missing `elements` property on `all` condition."
        | Some t when t ?= "any" ->
            match Json.tryGetArrayProperty "conditions" element with
            | Some cs ->
                cs
                |> List.map deserializeCondition
                |> collectResults
                |> Result.map Condition.Any
            | None -> Error "Missing `elements` property on `any` condition."
        | Some t when t ?= "notblank" ->
            match getId element with
            | Some id -> Ok <| Condition.NotBlank id
            | None -> Error "Missing `id` property on `notblank` condition."
        | Some t when t ?= "regex" ->
            match getId element, getString "pattern" element, Json.tryGetBoolProperty "caseInsensitive" element with
            | Some id, Some pattern, Some cs ->
                ({ Id = id
                   Pattern = pattern
                   CaseInsensitive = cs }: RegexMatchCondition)
                |> Condition.RegexMatch
                |> Ok
            | None, _, _ -> Error "Missing `id` property on `regex` condition."
            | _, None, _ -> Error "Missing `pattern` property on `regex` condition."
            | _, _, None -> Error "Missing `caseInsensitive` on `regex` condition."
        | Some t when t ?= "string" ->
            match getId element, getString "value" element, Json.tryGetBoolProperty "caseInsensitive" element with
            | Some id, Some v, Some cs ->
                ({ Id = id
                   Value = v
                   CaseInsensitive = cs }: StringMatchCondition)
                |> Condition.StringMatch
                |> Ok
            | None, _, _ -> Error "Missing `id` property on `regex` condition."
            | _, None, _ -> Error "Missing `pattern` property on `regex` condition."
            | _, _, None -> Error "Missing `caseInsensitive` on `regex` condition."
        | Some t -> Error $"Unknown condition type `{t}`."
        | None -> Error "Missing `type` property on condition."

    let rec deserializeBranch (element: JsonElement) =
        match getId element, Json.tryGetProperty "condition" element, Json.tryGetArrayProperty "elements" element with
        | Some id, Some condition, Some elements ->
            elements
            |> List.map (fun el ->
                match getType el with
                | Some t when t ?= "field" -> deserializeField el |> Result.map Element.Field
                | Some t when t ?= "branch" -> deserializeBranch el |> Result.map Element.Branch
                | Some t -> Error $"Unknown element type `{t}`."
                | None -> Error "Missing `type` property on element.")
            |> collectResults
            |> Result.bind (fun els ->
                deserializeCondition condition
                |> Result.map (fun c -> els, c))
            |> Result.map (fun (els, c) ->
                ({ Id = id
                   Condition = c
                   Elements = els }: Branch))
        | None, _, _ -> Error "Missing `id` property."
        | _, None, _ -> Error "Missing `condition` property."
        | _, _, None -> Error "Missing `elements` property."

    let deserializeElement (element: JsonElement) =
        match Json.tryGetStringProperty "type" element with
        | Some t when t |> equals "field" ->
            deserializeField element
            |> Result.map Element.Field
        | Some t when t ?= "branch" ->
            deserializeBranch element
            |> Result.map Element.Branch
        | Some t when t |> equals "branch" -> Error "todo implement."
        | Some t -> Error $"Unknown element type `{t}`."
        | None -> Error "Missing `type` property on element."

    let deserializePage (element: JsonElement) =
        match getId element, getString "title" element, Json.tryGetArrayProperty "elements" element with
        | Some id, Some title, Some els ->
            els
            |> List.map deserializeElement
            |> collectResults
            |> Result.map (fun elements ->
                { Id = id
                  Title = title
                  Elements = elements })
        | None, _, _ -> Error "Missing `id` property on page."
        | _, None, _ -> Error "Missing `title` property on page."
        | _, _, None -> Error "Missing `elements` property on page."

    let deserializeBody (element: JsonElement) =
        match getType element with
        | Some t when t ?= "pages" ->
            match Json.tryGetArrayProperty "pages" element with
            | Some ps ->
               ps
               |> List.map deserializePage
               |> collectResults
               |> Result.map FormBody.Pages
            | None -> Error $"Missing `elements` property on `elements` body."
        | Some t when t ?= "elements" ->
            match Json.tryGetArrayProperty "pages" element with
            | Some es ->
                es
                |> List.map deserializeElement
                |> collectResults
                |> Result.map FormBody.Elements
            | None -> Error $"Missing `elements` property on `elements` body."
        | Some t -> Error $"Unknown body type `{t}`."
        | None -> Error "Missing `type` property on body."
    
    let deserializeForm (element: JsonElement) =
        match getString "name" element, Json.tryGetProperty "body" element with
        | Some name, Some body ->
            deserializeBody body
            |> Result.map (fun b -> ({ Name = name; Body = b }: Form))
        | None, _ -> Error "Missing `name` property on form."
        | _, None -> Error "Missing `body` property on form."

    let deserialize json =
        match Json.tryOpenDocument json with
        | Ok jDoc -> deserializeForm jDoc.RootElement
        | Error e -> Error $"Failed to open json document. Error: `{e.Message}`"

    let deserializeFile path =
        match Json.tryLoadDocument path with
        | Ok jDoc -> deserializeForm jDoc.RootElement
        | Error e -> Error $"Failed to open json document. Error: `{e.Message}`"