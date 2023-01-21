using System.Text.Json;
using System.Text.Json.Serialization;
using Armory.Models.Enums;

namespace Armory.ViewModels;

[JsonConverter(typeof(ItemViewModelJsonConverter))]
public abstract class ItemViewModel
{
    public long Id { get; set; }

    public Guid TransactionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public RarityEnum Rarity { get; set; }

    public string RarityDescription { get; set; } = string.Empty;
}

public class ItemViewModelJsonConverter : JsonConverter<ItemViewModel>
{
    public override ItemViewModel? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var errors = new List<string>();

        try
        {
            return JsonSerializer.Deserialize<WeaponViewModel>(ref reader, options);
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }

        try
        {
            return JsonSerializer.Deserialize<ArmorViewModel>(ref reader, options);
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }

        var messages = string.Join(", ", errors.Select((value, index) => $"({index + 1}) {value}"));
        throw new JsonException($"OneOf: {messages}.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        ItemViewModel value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
