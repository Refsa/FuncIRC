namespace FuncIRC

open System

module MailboxProcessorHelpers =

    /// <summary>
    /// Creates a MailboxProcessor object with type 'T, issues the updateDelegate whenever new information is received
    /// </summary>
    let mailboxProcessorFactory<'T> (updateDelegate: 'T -> unit) =
        MailboxProcessor<'T>.Start
            (fun update ->
                let rec loop() = async {
                    let! newInfo = update.Receive()

                    updateDelegate newInfo

                    return! loop()
                }
                loop()
            )