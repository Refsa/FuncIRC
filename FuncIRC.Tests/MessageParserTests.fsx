#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

#load "TestMessages.fsx"

namespace FuncIRC.Tests

open NUnit.Framework

module MessageParserTests =
    open FuncIRC
    open FuncIRC.MessageParser
    open FuncIRC.MessageTypes
    open FuncIRC.Validators
    open TestMessages

    /// Prints contents of a Source type
    let printSource (source: Source) =
        printf "[ "
        if source.Nick.IsSome then
            printf "Nick = %s; " source.Nick.Value
        if source.User.IsSome then
            printf "User = %s; " source.User.Value
        if source.Host.IsSome then
            printf "Host = %s; " source.Host.Value
        printf " ]"

    /// Expanded equality comparison between two Message Records
    let messageEquals msg1 msg2 =
        match (msg1, msg2) with
        | _ when msg1.Tags   <> msg2.Tags   -> printfn "Tags not equal";   false
        | _ when msg1.Source <> msg2.Source -> printfn "Source not equal"; false
        | _ when msg1.Verb   <> msg2.Verb   -> printfn "Verb not equal";   false
        | _ when msg1.Params <> msg2.Params -> printfn "Params not equal"; false
        | _ -> true

    /// Function to print out content of a message to console
    let printMessage message =
        printf "\tTags: "
        if message.Tags.IsSome then 
            printf "%A" message.Tags.Value
        printf "\n\tSource: "
        if message.Source.IsSome then
            printSource message.Source.Value
        printf "\n\tVerb: "
        if message.Verb.IsSome then 
            printf "%s" (string message.Verb.Value)
        printf "\n\tParams: "
        if message.Params.IsSome then 
            printf "%A" message.Params.Value
        printfn ""

    /// Verifies that a message parsed with messageSplit is correct
    let verifyMessageParser (messageString: string, wantedResult: Message) =
        let parsedMessage: Message = parseMessageString messageString

        if (messageEquals parsedMessage wantedResult) then
            true
        else
            printfn "\nError on: %s" messageString

            printfn "Parsed Result:"
            printMessage parsedMessage

            printfn "Wanted Result:"
            printMessage wantedResult
            false

    [<Test>]
    let ``message parser should extract tags source verb and params``() =
        testMessages 
        |> List.iter 
            (fun tm -> 
                Assert.True(verifyMessageParser (tm.Input, tm.Output)
            ))

    [<Test>]
    let ``hostname validator should filter out invalid hostnames``() =
        hostnameTests
        |> List.iter
            (fun hn ->
                let result = (validateHostname hn.Hostname) = hn.Valid

                if not result then
                    printfn "Hostname %s was supposed to be %b" hn.Hostname hn.Valid

                Assert.True(result)
            )