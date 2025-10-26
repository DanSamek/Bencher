using System.ComponentModel.DataAnnotations;

namespace WebApplication.Web.Pages.Engines;

public class EngineFormModel
{
    [Required]
    public string Name { get; set; } = "";
        
    [Required]
    [Url]
    public string GitUrl { get; set; } = "";
        
    [Required]
    public string BuildScript { get; set; } = "";
}