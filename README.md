# FuncIRC - IRCv3 Client in F#

## Required
#### .Net Standard
Project uses .Net Standard 2.0 as referenced here: https://docs.microsoft.com/en-us/dotnet/standard/net-standard

#### Paket
Requires Paket to be installed as a system environment variable.  
See here for information: https://fsprojects.github.io/Paket/

## Setup
+ For first time setup of project run setup.cmd in root folder
+ To build the FuncIRC library run build_funcirc.cmd
+ To run the CLI use run_cli.cmd, it will also build the FuncIRC library

## Goals
+ IRC Client written in functional F#
+ It should follow the IRCv3 Client specifications
+ The Library DLL that comes out of the FuncIRC project should be interoperable with Unity Editor
+ It should work as a standalone library on .NET Standard 2.0 and above

## CI/CD
Currently a local gitlab runner is set up that runs on powershell. It's not the ideal way to handle the pipeline as it requries an active computer with all the required packages set up. Should create a docker image with all the required software setup so it can run on shared runners.

## Commit Messages
Structure: \[prefix\]: (\[message\]) <commit> <issue> <ms> <snippet> <mention>
+ \[\] - Required
+ <> - Optional

Description of semantics used below:
+ may - Optional
+ should - Required

#### Prefix
Each commit message should start with a prefix describing its purpose
+ project/ - related to project structure, should not contain any changes in functionality
+ irc/ - related to the FuncIRC client library
+ cli/ - Related to the CLI frontend for the irc library
+ refactor/ - When refactoring a piece of code, should not contain any changes in functionality
+ feature/ - A new feature that was added. Linked to issue and described in message
+ test/ - A new test case was added

Prefixes should be compunded together as such when it's appropriate:
+ irc/feature/:
+ cli/refactor/:
+ irc/project/:
+ irc/test/:

#### Message
After the prefix the message should appear. It prefarable should be kept between two parantheses "(Message)"

#### Commit
After the message a commit reference may appear

#### Issue
After the commit reference the issue related to the commit may appear

#### Milestone
After the issue a milestone tag may appear

#### Snippet
After the milestone tag the reference to a snippet may appear

#### Mention
At the end of the commit message any mentions may appear