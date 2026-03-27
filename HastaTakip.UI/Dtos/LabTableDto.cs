using System.Text.Json.Serialization;

namespace HastaTakip.UI.Dtos;

public class LabTableDto
{
    [JsonPropertyName("dates")]
    public List<string> Dates { get; set; } = new();

    [JsonPropertyName("rows")]
    public List<LabRowDto> Rows { get; set; } = new();
}

public class LabRowDto
{
    [JsonPropertyName("param")]
    public string Param { get; set; } = "";

    [JsonPropertyName("values")]
    public List<string?> Values { get; set; } = new();
}
