namespace Common.DTOs.DungeonEntrance;

public enum DungeonEntranceEventEnum
{

    RegisterEntrance = 1,
    RollbackCreate = 2,
    ChargeFee = 3,
    ProcessChargeFeeError = 4,
    ProcessRegistration = 5,
    RollbackChargeFee = 6,
}
