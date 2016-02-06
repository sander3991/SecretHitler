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
        Full = 0x07,
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

        PresidentActionExamine = 0x31,
        PresidentActionKill = 0x32,
        PresidentActionChoosePresident = 0x33,
        PresidentActionInvestigate = 0x34,


        PresidentActionExamineResponse = 0x41,
        PresidentActionKillResponse = 0x42,
        PresidentActionChoosePresidentResponse = 0x43,
        PresidentActionInvestigatePresidentResponse = 0x44,
    }
}
