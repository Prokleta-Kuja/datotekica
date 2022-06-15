using System.Web;
using datotekica.Entities;
using datotekica.Extensions;

namespace datotekica.Models;

public class InternalShareViewModel
{
    public InternalShareViewModel(InternalShareUser p)
    {
        if (p.InternalShare == null)
            throw new NullReferenceException($"{nameof(p.InternalShare)} must not be null");

        if (!Directory.Exists(p.InternalShare.Mount))
            throw new DirectoryNotFoundException($"Mount {p.InternalShare.Mount} does not exist");

        CanWrite = p.CanWrite;
        Root = new DirectoryInfo(p.InternalShare.Mount);
        Name = HttpUtility.HtmlEncode(p.InternalShare.Name);
        Modified = Root.LastWriteTimeUtc;
        Path = Root.FullName;
        Url = $"{C.Routes.InternalShare}/{HttpUtility.UrlPathEncode(p.InternalShare.Name)}";
        ModifiedAgo = Modified.ToTimeAgo();
    }

    public string Name { get; }
    public DirectoryInfo Root { get; }
    public bool CanWrite { get; }
    public string Url { get; set; }
    public string Path { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedAgo { get; set; }

    public override bool Equals(object? obj) => obj is InternalShareViewModel model && Name == model.Name;

    public override int GetHashCode() => Name.GetHashCode();
}