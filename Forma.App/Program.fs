open System.Text.Json
open Forma
open Forma.Forms
open Forma.StaticForms

let el = """
{
    "type": "field",
    "id": "input-3",
    "label": "Input 3",
    "subtitle": "Subtitle 3",
    "fieldType": {
        "type": "text",
        "textInput": "text"
    },
    "validation": [
        {
            "type": "notBlank",
            "message": ""
        },
        {
            "type": "regex",
            "pattern": "@email",
            "caseInsensitive": true,
            "message": "Must be a valid email address"
        }
    ]
}
"""

module JsonTest =
    
    
    
    let run _ =
        
        let r =  Serialization.deserializeFile "C:\\Users\\44748\\Projects\\__prototypes\\forms\\example_form.json"
        
        
        ()

//JsonTest.run ()


match Serialization.deserializeFile "C:\\Users\\44748\\Projects\\__prototypes\\forms\\example_form.json" with
| Ok form -> StaticFormsRenderer.run "C:\\Users\\44748\\Projects\\__prototypes\\forms" form
| Error e -> printfn $"Error! {e}"