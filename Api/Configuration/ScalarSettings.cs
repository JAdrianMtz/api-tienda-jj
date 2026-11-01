using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public class ScalarSettings
{
    public const string Section = "ScalarSettings";

    [Required]
    public required string Title { get; set; }
    [Required]
    public required string Version { get; set; }
    [Required]
    public required string Description { get; set; }

    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    [Url]
    public string? ContactUrl { get; set; }

    public string? LicenseName { get; set; }
    [Url]
    public string? LicenseUrl { get; set; }

    public bool Enabled { get; set; }
    [Required]
    public required string RedirectPath { get; set; }
}
