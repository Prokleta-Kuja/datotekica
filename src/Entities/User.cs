namespace datotekica.Entities;

public class User
{
    internal User(string usernameNormalized)
    {
        UsernameNormalized = usernameNormalized;
    }
    public int UserId { get; set; }
    public string UsernameNormalized { get; set; }
    public string TimezoneId { get; set; } = C.Env.TimeZone;
    public string LocaleId { get; set; } = C.Env.Locale;
    public DateTime? Disabled { get; set; }

    public virtual ICollection<InternalShareUser> InternalShareUsers { get; set; } = new HashSet<InternalShareUser>();
}