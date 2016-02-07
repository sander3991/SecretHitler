namespace SecretHitler.Networking
{
    public enum ServerCommands : byte
    {
        None = 0x00,
        OK = 0x01,
        Connect = 0x02,
        Message = 0x03,
        ReceiveMessage = 0x04,
        PlayerConnected = 0x05,
        PlayerDisconnected = 0x06,
        SendGameState = 0x08,
        GetGameState = 0x09,
        AnnounceCard = 0x0A,
        RevealRole = 0x0B,
        Multiple = 0x0C,
        AnnouncePresident = 0x0D,
        AnnounceChancellor = 0x0E,
        CastVote = 0x0F,
        PlayerVoted = 0x10,
        AnnounceVotes = 0x11,
        PolicyCardsDrawn = 0x12,
        PresidentPickPolicyCard = 0x13,
        ChancellorPickPolicyCard = 0x14,
        PresidentPolicyCardPicked = 0x15,
        ChancellorPolicyCardPicked = 0x16,
        PresidentDiscarded = 0x17,
        ChancellorDiscarded = 0x18,
        CardPlayed = 0x19,
        PresidentAction = 0x1A,
        PresidentDoingAction = 0x1B,
        RevealMembership = 0x1C,
        KillPlayer = 0x1D,
        NotHitler = 0x1E,
        IncrementElectionTracker = 0x1F,
        ResetElectionTracker = 0x20,

        ChancellorRequestVeto = 0x21, //Chancellor to server to request veto
        PresidentConfirmVeto = 0x22, //Server to All to notify of a veto request
        PresidentRequestVetoAllowed = 0x23, //President to Server to notify if approved
        AnnounceVetoResult = 0x24, //Server to all to notify result of veto

        PresidentActionExamine = 0x31,
        PresidentActionKill = 0x32,
        PresidentActionChoosePresident = 0x33,
        PresidentActionInvestigate = 0x34,


        PresidentActionExamineResponse = 0x41,
        PresidentActionKillResponse = 0x42,
        PresidentActionChoosePresidentResponse = 0x43,
        PresidentActionInvestigatePresidentResponse = 0x44,

        FascistWin = 0x51,
        LiberalWin = 0x52,

        CurrentlyPlaying = 0x60,
        Full = 0x61,
    }
}
