open Forma.Forms
open Forma.StaticForms


[ ({ Id = "page-1"
     Title = "Page 1"
     Elements =
       [ ({ Id = "input-1"
            Label = "Input 1"
            Subtitle = Some "Test input 1"
            Type = FieldType.Text TextInput.Text
            Validation =
              [ ValidationType.NotBlank "Input 1 can not be blank"
                ValidationType.RegexMatch(
                    """/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/""",
                    true,
                    "Must be a valid email address"
                ) ] }: Field)
         |> Element.Field
         ({ Id = "input-2"
            Label = "Input 2"
            Subtitle = Some "Test input 2"
            Type = FieldType.Text TextInput.Text
            Validation = [ ValidationType.NotBlank "Input 2 can not be blank" ] }: Field)
         |> Element.Field
         ({ Id = "test-branch"
            Condition =
              Condition.All [ Condition.NotBlank "input-1"
                              Condition.NotBlank "input-2" ]
            Elements =
              [ ({ Id = "branch-1-input-1"
                   Label = "Input 1 - 1"
                   Subtitle = Some "Another test input"
                   Type = FieldType.Text TextInput.Text
                   Validation = [] }: Field)
              |> Element.Field ] })
         |> Element.Branch ] }: FormPage)
  ({ Id = "page-2"
     Title = "Page 2"
     Elements =
       [ ({ Id = "input-3"
            Label = "Input 3"
            Subtitle = Some "Test input 3"
            Type = FieldType.Text TextInput.Text
            Validation =
              [ ValidationType.NotBlank "Input 3 can not be blank"
                ValidationType.RegexMatch(
                    """/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/""",
                    true,
                    "Must be a valid email address"
                ) ] }: Field)
         |> Element.Field
         ({ Id = "input-4"
            Label = "Input 4"
            Subtitle = Some "Test input 4"
            Type = FieldType.Text TextInput.Text
            Validation = [ ValidationType.NotBlank "Input 4 can not be blank" ] }: Field)
         |> Element.Field ] }: FormPage) ]
|> fun ps -> ({ Name = "Example form 1"; Body = FormBody.Pages ps  }: Form)
|> StaticFormsRenderer.run "C:\\Users\\44748\\Projects\\__prototypes\\forms"