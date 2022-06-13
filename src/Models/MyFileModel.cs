using System.Web;
using datotekica.Extensions;

namespace datotekica.Models;

public class MyFileModel
{
    public MyFileModel(FileInfo file, string prevPath)
    {
        Name = HttpUtility.HtmlEncode(file.Name);
        Modified = file.LastWriteTimeUtc;
        Size = file.Length;
        Path = file.FullName;
        Url = $"{prevPath}/{HttpUtility.UrlPathEncode(file.Name)}";
        ModifiedAgo = Modified.ToTimeAgo();
    }

    public string Url { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedAgo { get; set; }
    public long Size { get; set; }
}