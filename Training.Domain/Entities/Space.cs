using Training.Domain.Enums;
using Training.Domain.Exceptions;

namespace Training.Domain.Entities;

public class Space
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public SpaceType Type { get; private set; }
    public int? Capacity { get; private set; }
    public bool SupportsSeatManagement { get; private set; }
    public bool IsActive { get; private set; }

    private Space()
    {
    }

    public Space(
        string name,
        string code,
        string? description,
        SpaceType type,
        int? capacity,
        bool supportsSeatManagement,
        bool isActive)
    {
        string normalizedCode = NormalizeCode(code);
        Validate(name, normalizedCode, capacity);

        Id = Guid.NewGuid();
        Name = name;
        Code = normalizedCode;
        Description = description;
        Type = type;
        Capacity = capacity;
        SupportsSeatManagement = supportsSeatManagement;
        IsActive = isActive;
    }

    public void Update(
        string name,
        string code,
        string? description,
        SpaceType type,
        int? capacity,
        bool supportsSeatManagement,
        bool isActive)
    {
        string normalizedCode = NormalizeCode(code);
        Validate(name, normalizedCode, capacity);

        Name = name;
        Code = normalizedCode;
        Description = description;
        Type = type;
        Capacity = capacity;
        SupportsSeatManagement = supportsSeatManagement;
        IsActive = isActive;
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpper();
    }

    private static void Validate(string name, string code, int? capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidSpaceNameException();
        }

        if (name.Length > 150)
        {
            throw new InvalidSpaceNameException();
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidSpaceCodeException();
        }

        if (code.Length > 100)
        {
            throw new InvalidSpaceCodeException();
        }

        if (capacity.HasValue && capacity.Value <= 0)
        {
            throw new InvalidSpaceCapacityException();
        }
    }
}
