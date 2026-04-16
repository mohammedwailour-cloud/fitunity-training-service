using Training.Domain.Enums;

namespace Training.Application.Spaces.DTOs;

public class UpdateSpaceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SpaceType Type { get; set; }
    public int? Capacity { get; set; }
    public bool SupportsSeatManagement { get; set; }
    public bool IsActive { get; set; }
}
