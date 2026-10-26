using System.Text.Json.Serialization;

namespace Api.Dtos;

public class LoginResponseDto
{
    public required string AccessToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonIgnore]
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}
