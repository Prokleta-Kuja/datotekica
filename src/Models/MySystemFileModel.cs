using System.Web;
using datotekica.Extensions;

namespace datotekica;

public class MySystemFileModel
{
    public MySystemFileModel(FileSystemInfo file, string prevPath, string? query = null)
    {
        Name = file.Name;
        Modified = file.LastWriteTimeUtc;
        Path = file.FullName;
        Url = $"{prevPath}/{HttpUtility.UrlPathEncode(file.Name)}{query}";
        ModifiedAgo = Modified.ToTimeAgo();
    }
    public string Url { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
    public DateTime Modified { get; set; }
    public string ModifiedAgo { get; set; }
}

public class MyDirectoryModel : MySystemFileModel
{
    public MyDirectoryModel(DirectoryInfo dir, string prevPath, string? query = null) : base(dir, prevPath, query) { }
}

public class MyFileModel : MySystemFileModel
{
    public MyFileModel(FileInfo file, string prevPath, string? query = null) : base(file, prevPath, query)
    {
        Size = file.Length;
    }

    public long Size { get; set; }
}