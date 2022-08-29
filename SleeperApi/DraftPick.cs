namespace DraftDominatorSleeperLiveDraftSync.SleeperApi;

public record DraftPick(
    int Round,
    string? RosterId,
    string PlayerId,
    string PickedBy,
    int PickNo,
    Metadata Metadata,
    bool? IsKeeper,
    int DraftSlot,
    string DraftId);