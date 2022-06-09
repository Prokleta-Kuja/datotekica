using System.Web;

namespace datotekica.Models;

public class MyDirectoryModel
{
    public MyDirectoryModel(DirectoryInfo dir, string prevPath)
    {
        Name = HttpUtility.HtmlEncode(dir.Name);
        Modified = dir.LastWriteTimeUtc;
        Url = $"{prevPath}/{HttpUtility.UrlPathEncode(dir.Name)}";
    }

    public string Url { get; set; }
    public string Name { get; set; }
    public DateTime Modified { get; set; }
}