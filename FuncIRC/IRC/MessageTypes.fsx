namespace FuncIRC

module MessageTypes =
    type Tag =
        { Key: string
          Value: string option }

    type Verb = Verb of string

    type Parameter = Parameter of string
    type Parameters = Parameters of Parameter list

    type Source =
        { Nick: string option
          User: string option
          Host: string option }

    type Message =
        { Tags: Tag list option
          Source: Source option
          Verb: Verb option
          Params: Parameters option }

    /// Takes a string option and transforms it to a Verb type
    let toVerb input =
        match input with
        | Some input -> Some (Verb input)
        | None -> None

    /// transforms a collection of strings into a Parameters type
    let toParameters input =
        Parameters [ for s in input -> Parameter s ]