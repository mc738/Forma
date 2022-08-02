namespace Forma

module Forms =

    type Form = {
        Name: string
        Body: FormBody
    }

    and FormBody =
        | Pages of FormPage list
        | Elements of Element list
    
    and FormPage =
        { Id: string
          Title: string
          Elements: Element list }

    and Element =
        | Field of Field
        | Branch of Branch

    and Field =
        { Id: string
          Label: string
          Subtitle: string option
          Type: FieldType
          Validation: ValidationType list }

    and Branch =
        { Id: string
          Elements: Element list
          Condition: Condition }

    and FieldType =
        | Text of TextInput
        | TextArea
        | Selector of SelectorInput

    and [<RequireQualifiedAccess>] TextInput =
        | Text
        | Integer
        | Decimal
        | Email
        | Phone

    and [<RequireQualifiedAccess>] SelectorInput = { Values: SelectorValue list }

    and SelectorValue = { Name: string; Value: string }

    and [<RequireQualifiedAccess>] Condition =
        | NotBlank of Id: string
        | RegexMatch of RegexMatchCondition
        | StringMatch of StringMatchCondition
        | Any of Condition list
        | All of Condition list
        
    and RegexMatchCondition = {
        Id: string
        Pattern: string
        CaseInsensitive: bool
    }
    
    and StringMatchCondition = {
        Id: string
        Value: string
        CaseInsensitive: bool
    } 

    and [<RequireQualifiedAccess>] ValidationType =
        | NotBlank of Message: string
        | RegexMatch of Pattern: string * CaseInsensitive: bool * Message: string
        | StringMatch of Value: string * CaseInsensitive: bool * Message: string
    
    
    
    

