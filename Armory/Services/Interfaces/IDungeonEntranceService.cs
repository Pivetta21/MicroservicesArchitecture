using Armory.Models.Enums;
using Armory.ViewModels;
using Common.DTOs.DungeonEntrance;
using FluentResults;
using RabbitMQ.Client;

namespace Armory.Services.Interfaces;

public interface IDungeonEntranceService
{
    Task<Result<DungeonEntranceViewModel>> RegisterEntrance(DungeonRegisterEntranceViewModel body);

    Task ProcessDungeonEntrance(DungeonEntranceGameDto dto, SagaInfo sagaInfo, IBasicProperties props);

    Task<IEnumerable<DungeonEntranceViewModel>> Get(long? characterId, DungeonEntranceStatusEnum? status);
}
