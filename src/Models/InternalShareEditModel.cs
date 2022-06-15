using datotekica.Entities;

namespace datotekica.Models;

public class InternalShareEditModel
{
    public InternalShareEditModel(InternalShare share)
    {
        InternalShareId = share.InternalShareId;
        Mount = share.Mount;
        Name = share.Name;
    }

    public int InternalShareId { get; set; }
    public string? Mount { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, string>? Validate()
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Mount))
            errors.Add(nameof(Mount), "Required");
        else if (!Directory.Exists(Mount))
            errors.Add(nameof(Mount), "Does not exist");

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), "Required");

        return errors.Any() ? errors : null;
    }
}