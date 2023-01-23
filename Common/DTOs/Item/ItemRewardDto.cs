namespace Common.DTOs.Item;

public class ItemRewardDto
{
    public required string Name { get; set; }

    public required Guid TransactionId { get; set; }

    public required int Rarity { get; set; }

    public required long Price { get; set; }

    public int? Resistance { get; set; }

    public int? Power { get; set; }
}
