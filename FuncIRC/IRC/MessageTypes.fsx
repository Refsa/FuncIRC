#load "../Utils/StringHelpers.fsx"

namespace FuncIRC

open StringHelpers

module MessageTypes =
    type Tag =
        { 
            Key: string
            Value: string option 
        }

    type Verb = 
        | Verb of string
        member x.Value = let (Verb verb) = x in verb

    type Parameter = 
        | Parameter of string
        member x.Value = let (Parameter parameter) = x in parameter

    type Parameters =
        | Parameters of Parameter array
        member x.Value = let (Parameters parameters) = x in parameters

    type Source =
        { 
            Nick: string option
            User: string option
            Host: string option 
        }

    type Message =
        { 
            Tags: Tag list option
            Source: Source option
            Verb: Verb option
            Params: Parameters option 
        } with
        static member NewSimpleMessage verb parameters        = {Tags = None; Source = None; Verb = verb; Params = parameters}
        static member NewSourceMessage source verb parameters = {Tags = None; Source = source; Verb = verb; Params = parameters}
        static member NewMessage tags source verb parameters  = {Tags = tags; Source = source; Verb = verb; Params = parameters}

//#region ToString methods
    type Tag with
        member this.ToString =
            this.Key + "=" + (stringFromStringOption this.Value)

    type Verb with
        member this.ToString = this.Value

    type Parameter with
        member this.ToString = this.Value

    type Parameters with
        member this.ToString = 
            let rec buildString (paramsString: string) (index: int) (last: int) =
                match index = last with
                | true -> paramsString
                | false -> 
                    buildString (paramsString + " " + this.Value.[index].Value) (index + 1) last

            buildString "" 0 this.Value.Length

    type Source with
        member this.ToString =
            ":" + stringFromStringOption this.Nick + 
            "!" + stringFromStringOption this.User + 
            "@" + stringFromStringOption this.Host

    type Message with
        member this.ToMessageString =
            match this.Tags with
            | Some tags -> tags.ToString()
            | None -> ""
            +
            match this.Source with
            | Some source -> source.ToString
            | None -> ""
            +
            match this.Verb with
            | Some verb -> verb.ToString
            | None -> ""
            +
            match this.Params with
            | Some parameters -> parameters.ToString
            | None -> ""
//#endregion

//#region ToType functions
    /// Takes a string option and transforms it to a Verb type
    let toVerb input =
        match input with
        | Some input -> Some (Verb input)
        | None -> None

    /// transforms a collection of strings into a Parameters type
    let toParameters input =
        Parameters [| for s in input -> Parameter s |]

    /// Attempts to cast Verb type to int, returns None if it fails
    let verbToInt (verb: Verb) = 
        try
            Some (int (verb.Value))
        with
        | _ -> None
//#endregion