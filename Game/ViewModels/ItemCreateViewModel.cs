using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Models.Enums;

namespace Game.ViewModels;

[JsonConverter(typeof(ItemCreateViewModelJsonConverter))]
public abstract class ItemCreateViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public RarityEnum Rarity { get; set; }

    [Required]
    public int MaxQuality { get; set; }

    [Required]
    public long Price { get; set; }
}

public class ItemCreateViewModelJsonConverter : JsonConverter<ItemCreateViewModel>
{
    public override ItemCreateViewModel? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var errors = new List<string>();

        try
        {
            return JsonSerializer.Deserialize<WeaponCreateViewModel>(ref reader, options);
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }

        try
        {
            return JsonSerializer.Deserialize<ArmorCreateViewModel>(ref reader, options);
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
        ItemCreateViewModel value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
