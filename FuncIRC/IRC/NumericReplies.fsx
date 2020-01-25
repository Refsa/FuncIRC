namespace FuncIRC

/// All Numerics names mapped to their numeric values
module NumericReplies =

    type NumericsReplies =
        | NO_NAME 
        // Replies
        | RPL_WELCOME
        | RPL_YOURHOST
        | RPL_CREATED
        | RPL_MYINFO
        | RPL_ISUPPORT
        | RPL_BOUNCE
        | RPL_UMODEIS
        | RPL_LUSERCLIENT
        | RPL_LUSEROP
        | RPL_LUSERUNKNOWN
        | RPL_LUSERCHANNELS
        | RPL_LUSERME
        | RPL_ADMINME
        | RPL_ADMINLOC1
        | RPL_ADMINLOC2
        | RPL_ADMINEMAIL
        | RPL_TRYAGAIN
        | RPL_LOCALUSERS
        | RPL_GLOBALUSERS
        | RPL_WHOISCERTFP
        | RPL_NONE
        | RPL_AWAY
        | RPL_USERHOST
        | RPL_ISON
        | RPL_UNAWAY
        | RPL_NOWAWAY
        | RPL_WHOISUSER
        | RPL_WHOISSERVER
        | RPL_WHOWASUSER
        | RPL_WHOISOPERATOR
        | RPL_WHOISIDLE
        | RPL_ENDOFWHOIS
        | RPL_WHOISCHANNELS
        | RPL_LISTSTART
        | RPL_LIST
        | RPL_LISTEND
        | RPL_CHANNELMODEIS
        | RPL_CREATIONTIME
        | RPL_NOTOPIC
        | RPL_TOPIC
        | RPL_TOPICWHOTIME
        | RPL_INVITING
        | RPL_INVITELIST
        | RPL_ENDOFINVITELIST
        | RPL_EXCEPTLIST
        | RPL_ENDOFEXCEPTLIST
        | RPL_VERSION
        | RPL_NAMREPLY
        | RPL_ENDOFNAMES
        | RPL_BANLIST
        | RPL_ENDOFBANLIST
        | RPL_ENDOFWHOWAS
        | RPL_MOTDSTART
        | RPL_MOTD
        | RPL_ENDOFMOTD
        | RPL_YOUREOPER
        | RPL_REHASHING
        | RPL_STARTTLS
        | RPL_LOGGEDIN
        | RPL_LOGGEDOUT
        | RPL_SASLSUCCESS
        // Errors
        | ERR_UNKNOWNERROR
        | ERR_NOSUCHNICK
        | ERR_NOSUCHSERVER
        | ERR_NOSUCHCHANNEL
        | ERR_CANNOTSENDTOCHAN
        | ERR_TOOMANYCHANNELS
        | ERR_UNKNOWNCOMMAND
        | ERR_NOMOTD
        | ERR_ERRONEUSNICKNAME
        | ERR_NICKNAMEINUSE
        | ERR_USERNOTINCHANNEL
        | ERR_NOTONCHANNEL
        | ERR_USERONCHANNEL
        | ERR_NOTREGISTERED
        | ERR_NEEDMOREPARAMS
        | ERR_ALREADYREGISTERED
        | ERR_PASSWDMISMATCH
        | ERR_YOUREBANNEDCREEP
        | ERR_CHANNELISFULL
        | ERR_UNKNOWNMODE
        | ERR_INVITEONLYCHAN
        | ERR_BANNEDFROMCHAN
        | ERR_BADCHANNELKEY
        | ERR_NOPRIVILEGES
        | ERR_CHANOPRIVSNEEDED
        | ERR_CANTKILLSERVER
        | ERR_NOOPERHOST
        | ERR_UMODEUNKNOWNFLAG
        | ERR_USERSDONTMATCH
        | ERR_STARTTLS
        | ERR_NOPRIVS
        | ERR_NICKLOCKED
        | ERR_SASLFAIL
        | ERR_SASLTOOLONG
        | ERR_SASLABORTED
        | ERR_SASLALREADY
        | ERR_SASLMECHS
        // Verbs
        | MSG_PING
        | MSG_PRIVMSG
        | MSG_NOTICE
        | MSG_ERROR
        | MSG_JOIN


    let numericReplies =
        [ 001, NumericsReplies.RPL_WELCOME
          002, NumericsReplies.RPL_YOURHOST
          003, NumericsReplies.RPL_CREATED
          004, NumericsReplies.RPL_MYINFO
          005, NumericsReplies.RPL_ISUPPORT
          010, NumericsReplies.RPL_BOUNCE
          221, NumericsReplies.RPL_UMODEIS
          251, NumericsReplies.RPL_LUSERCLIENT
          252, NumericsReplies.RPL_LUSEROP
          253, NumericsReplies.RPL_LUSERUNKNOWN
          254, NumericsReplies.RPL_LUSERCHANNELS
          255, NumericsReplies.RPL_LUSERME
          256, NumericsReplies.RPL_ADMINME
          257, NumericsReplies.RPL_ADMINLOC1
          258, NumericsReplies.RPL_ADMINLOC2
          259, NumericsReplies.RPL_ADMINEMAIL
          263, NumericsReplies.RPL_TRYAGAIN
          265, NumericsReplies.RPL_LOCALUSERS
          266, NumericsReplies.RPL_GLOBALUSERS
          276, NumericsReplies.RPL_WHOISCERTFP
          300, NumericsReplies.RPL_NONE
          301, NumericsReplies.RPL_AWAY
          302, NumericsReplies.RPL_USERHOST
          303, NumericsReplies.RPL_ISON
          305, NumericsReplies.RPL_UNAWAY
          306, NumericsReplies.RPL_NOWAWAY
          311, NumericsReplies.RPL_WHOISUSER
          312, NumericsReplies.RPL_WHOISSERVER
          313, NumericsReplies.RPL_WHOISOPERATOR
          314, NumericsReplies.RPL_WHOWASUSER
          317, NumericsReplies.RPL_WHOISIDLE
          318, NumericsReplies.RPL_ENDOFWHOIS
          319, NumericsReplies.RPL_WHOISCHANNELS
          321, NumericsReplies.RPL_LISTSTART
          322, NumericsReplies.RPL_LIST
          323, NumericsReplies.RPL_LISTEND
          324, NumericsReplies.RPL_CHANNELMODEIS
          329, NumericsReplies.RPL_CREATIONTIME
          331, NumericsReplies.RPL_NOTOPIC
          332, NumericsReplies.RPL_TOPIC
          333, NumericsReplies.RPL_TOPICWHOTIME
          341, NumericsReplies.RPL_INVITING
          346, NumericsReplies.RPL_INVITELIST
          347, NumericsReplies.RPL_ENDOFINVITELIST
          348, NumericsReplies.RPL_EXCEPTLIST
          349, NumericsReplies.RPL_ENDOFEXCEPTLIST
          351, NumericsReplies.RPL_VERSION
          353, NumericsReplies.RPL_NAMREPLY
          366, NumericsReplies.RPL_ENDOFNAMES
          367, NumericsReplies.RPL_BANLIST
          368, NumericsReplies.RPL_ENDOFBANLIST
          369, NumericsReplies.RPL_ENDOFWHOWAS
          375, NumericsReplies.RPL_MOTDSTART
          372, NumericsReplies.RPL_MOTD
          376, NumericsReplies.RPL_ENDOFMOTD
          381, NumericsReplies.RPL_YOUREOPER
          382, NumericsReplies.RPL_REHASHING
          670, NumericsReplies.RPL_STARTTLS
          900, NumericsReplies.RPL_LOGGEDIN
          901, NumericsReplies.RPL_LOGGEDOUT
          903, NumericsReplies.RPL_SASLSUCCESS
          400, NumericsReplies.ERR_UNKNOWNERROR
          401, NumericsReplies.ERR_NOSUCHNICK
          402, NumericsReplies.ERR_NOSUCHSERVER
          403, NumericsReplies.ERR_NOSUCHCHANNEL
          404, NumericsReplies.ERR_CANNOTSENDTOCHAN
          405, NumericsReplies.ERR_TOOMANYCHANNELS
          412, NumericsReplies.ERR_UNKNOWNCOMMAND
          422, NumericsReplies.ERR_NOMOTD
          432, NumericsReplies.ERR_ERRONEUSNICKNAME
          433, NumericsReplies.ERR_NICKNAMEINUSE
          441, NumericsReplies.ERR_USERNOTINCHANNEL
          442, NumericsReplies.ERR_NOTONCHANNEL
          443, NumericsReplies.ERR_USERONCHANNEL
          451, NumericsReplies.ERR_NOTREGISTERED
          461, NumericsReplies.ERR_NEEDMOREPARAMS
          462, NumericsReplies.ERR_ALREADYREGISTERED
          464, NumericsReplies.ERR_PASSWDMISMATCH
          465, NumericsReplies.ERR_YOUREBANNEDCREEP
          471, NumericsReplies.ERR_CHANNELISFULL
          472, NumericsReplies.ERR_UNKNOWNMODE
          473, NumericsReplies.ERR_INVITEONLYCHAN
          474, NumericsReplies.ERR_BANNEDFROMCHAN
          475, NumericsReplies.ERR_BADCHANNELKEY
          481, NumericsReplies.ERR_NOPRIVILEGES
          482, NumericsReplies.ERR_CHANOPRIVSNEEDED
          483, NumericsReplies.ERR_CANTKILLSERVER
          491, NumericsReplies.ERR_NOOPERHOST
          501, NumericsReplies.ERR_UMODEUNKNOWNFLAG
          502, NumericsReplies.ERR_USERSDONTMATCH
          691, NumericsReplies.ERR_STARTTLS
          723, NumericsReplies.ERR_NOPRIVS
          902, NumericsReplies.ERR_NICKLOCKED
          904, NumericsReplies.ERR_SASLFAIL
          905, NumericsReplies.ERR_SASLTOOLONG
          906, NumericsReplies.ERR_SASLABORTED
          907, NumericsReplies.ERR_SASLALREADY
          908, NumericsReplies.ERR_SASLMECHS ]
        |> Map.ofList
