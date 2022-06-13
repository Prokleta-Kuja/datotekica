namespace datotekica.Entities;

public class InternalShare
{
    internal InternalShare(string mount, string name)
    {
        Mount = mount;
        Name = name;
    }
    public int InternalShareId { get; set; }
    public string Mount { get; set; }
    public string Name { get; set; }

    public virtual ICollection<InternalShareUser> InternalShareUsers { get; set; } = new HashSet<InternalShareUser>();
}