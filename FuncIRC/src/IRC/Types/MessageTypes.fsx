#load "../../Utils/StringHelpers.fsx"

namespace FuncIRC

open StringHelpers

module MessageTypes =
    /// <summary>
    /// Record to store the Key-Value-Pair of a Tag from an IRC message
    /// </summary>
    type Tag =
        { 
            Key: string
            Value: string option 
        }

    /// <summary>
    /// Single-Case DU to hold the Verb part of an IRC message
    /// </summary>
    type Verb = 
        | Verb of string
        member x.Value = let (Verb verb) = x in verb

    /// <summary>
    /// Single-Case DU to wrap a Parameter of an IRC message
    /// </summary>
    type Parameter = 
        | Parameter of string
        member x.Value = let (Parameter parameter) = x in parameter

    /// <summary>
    /// Holds the Parameters from an IRC message
    /// </summary>
    type Parameters =
        | Parameters of Parameter array
        member x.Value = let (Parameters parameters) = x in parameters

    /// <summary>
    /// Holds the Source part of an IRC message
    /// </summary>
    type Source =
        { 
            Nick: string option
            User: string option
            Host: string option 
        }

    /// <summary>
    /// Record to hold an IRC message after it has been parsed from string
    /// </summary>
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
        static member EmptyMessage = {Tags = None; Source = None; Verb = None; Params = None}

//#region ToString methods
    /// <summary>
    /// Creates a string from a Tag DU
    /// </summary>
    type Tag with
        member this.ToString =
            this.Key + "=" + (stringFromStringOption this.Value)

    /// <summary>
    /// Creates a string from a Verb DU
    /// </summary>
    type Verb with
        member this.ToString = this.Value

    /// <summary>
    /// Creates a string from a Parameter DU
    /// </summary>
    type Parameter with
        member this.ToString = this.Value

    /// <summary>
    /// Creates a string from a Parameters DU
    /// </summary>
    type Parameters with
        member this.ToString = 
            let rec buildString (paramsString: string) (index: int) (last: int) =
                match index = last with
                | true -> paramsString
                | false -> 
                    buildString (paramsString + " " + this.Value.[index].Value) (index + 1) last

            buildString "" 0 this.Value.Length

    /// <summary>
    /// Creates a string from a Source record
    /// </summary>
    type Source with
        member this.ToString =
            ":" + stringFromStringOption this.Nick + 
            "!" + stringFromStringOption this.User + 
            "@" + stringFromStringOption this.Host

    /// <summary>
    /// Creates a string from a Message record
    /// </summary>
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

    /// <summary>
    /// Construct a single outboud IRC message from a list of messages
    /// </summary>
    let messagesToString (messages: Message list) =
        let mutable outboundMessage = ""
        messages |> List.iter (fun (m: Message) -> outboundMessage <- outboundMessage + m.ToMessageString + "\r\n" )
        outboundMessage
//#endregion

//#region ToType functions
    /// <summary>
    /// Takes a string option and transforms it to a Verb type
    /// </summary>
    let toVerb input =
        match input with
        | Some input -> Some (Verb input)
        | None -> None

    /// <summary>
    /// transforms a collection of strings into a Parameters type
    /// </summary>
    let toParameters input =
        Parameters [| for s in input -> Parameter s |]

    /// <summary>
    /// Attempts to cast Verb type to int, returns None if it fails
    /// </summary>
    let verbToInt (verb: Verb) = 
        try
            Some (int (verb.Value))
        with
        | _ -> None
//#endregion

//#region Verb extensions
    let (|IsNumeric|IsVerbName|) (verb: Verb) =
        let numeric = verbToInt verb

        match numeric with
        | Some numeric -> IsNumeric numeric
        | None -> IsVerbName verb.Value
//#endregion