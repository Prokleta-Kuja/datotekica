namespace datotekica.Models;

public class InternalShareCreateModel
{
    public string? Mount { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, string>? Validate()
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Mount))
            errors.Add(nameof(Mount), "Required");

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add(nameof(Name), "Required");

        return errors.Any() ? errors : null;
    }
}