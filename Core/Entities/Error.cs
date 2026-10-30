namespace Core.Entities;

public class Error
{
    public Guid Id { get; set; }
    public required string ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public DateTime OcurredAtUtc { get; set; }
}
