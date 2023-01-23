namespace Armory.Models.Enums;

public enum DungeonEntranceStatusEnum
{
    RegistrationRequested = 1,
    RegistrationFailed = 2,
    ReadyToUse = 3,
    AwaitingProcessing = 4,
    Processed = 5,
    ProcessedWithError = 6,
}
