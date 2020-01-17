module Modes =
    type UserMode =
        | INVISIBLE
        | OPER
        | LOCAL_OPER
        | REGISTERED
        | WALLOPS

    type ChannelMode =
        | BAN
        | EXCEPTION
        | CLIENT_LIMIT
        | INVITE_ONLY
        | INVITE_EXCEPTION
        | KEY
        | MODERATED
        | SECRET
        | PROTECTED
        | NO_EXTERNAL_MESSAGE

    type ChannelMembershipPrefix =
        | FOUNDER
        | PROTECTED
        | OPERATOR
        | HALFOP
        | VOICE

    let channelMembershipPrefixMap =
        [
            ChannelMembershipPrefix.FOUNDER, ["+q"];
            ChannelMembershipPrefix.PROTECTED, ["+a"];
            ChannelMembershipPrefix.OPERATOR, ["+o"; "%"];
            ChannelMembershipPrefix.HALFOP, ["+h"; "%"];
            ChannelMembershipPrefix.VOICE, ["+v"; "+"]
        ] |> Map.ofList

    let channelModeMap = 
        [
            ChannelMode.BAN, "+b";
            ChannelMode.EXCEPTION, "+e";
            ChannelMode.CLIENT_LIMIT, "+l";
            ChannelMode.INVITE_ONLY, "+i";
            ChannelMode.INVITE_EXCEPTION, "+I";
            ChannelMode.KEY, "+k";
            ChannelMode.MODERATED, "+m";
            ChannelMode.SECRET, "+s";
            ChannelMode.PROTECTED, "+t";
            ChannelMode.NO_EXTERNAL_MESSAGE, "+n"
        ] |> Map.ofList

    let userModeMap =
        [
            UserMode.INVISIBLE, "+i";
            UserMode.OPER, "+o";
            UserMode.LOCAL_OPER, "+O";
            UserMode.REGISTERED, "+r";
            UserMode.WALLOPS, "+w"
        ] |> Map.ofList