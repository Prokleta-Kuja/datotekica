using System.Web;
using datotekica.Extensions;

namespace datotekica.Models;

public class MyDirectoryModel
{
    public MyDirectoryModel(DirectoryInfo dir, string prevPath)
    {
        Name = dir.Name;
        Modified = dir.LastWriteTimeUtc;
        Path = dir.FullName;
        Url = $"{prevPath}/{HttpUtility.UrlPathEncode(dir.Name)}";
        ModifiedAgo = Modified.ToTimeAgo();
    }

    public string Url { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedAgo { get; set; }
}