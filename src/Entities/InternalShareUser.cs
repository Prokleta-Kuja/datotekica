namespace datotekica.Entities;

public class InternalShareUser
{
    internal InternalShareUser(int userId, bool canWrite)
    {
        UserId = userId;
        CanWrite = canWrite;
    }
    public int InternalShareId { get; set; }
    public int UserId { get; set; }
    public bool CanWrite { get; set; }

    public InternalShare? InternalShare { get; set; }
    public User? User { get; set; }
}