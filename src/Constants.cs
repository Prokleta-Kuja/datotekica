using System.Globalization;
using System.Text.Json;

namespace datotekica;

public static class C
{
    public static readonly TimeZoneInfo DefaultTZ = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zagreb");
    public static readonly CultureInfo DefaultLocale = CultureInfo.GetCultureInfo("en-US");
    static HashSet<string>? _adminUsers;
    public static bool IsAdmin(string username)
    {
        if (_adminUsers == null)
        {
            _adminUsers = new(StringComparer.InvariantCultureIgnoreCase);
            var users = C.Env.AdminUsers.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var user in users)
                _adminUsers.Add(user);
        }

        return _adminUsers.Contains(username);
    }
    public static string GetHumanFileSize(FileInfo file)
    {
        var sizes = new string[] { "B", "KB", "MB", "GB", "TB" };
        double len = file.Length;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
        // show a single decimal place, and no space.
        return String.Format("{0:0.##} {1}", len, sizes[order]);
    }
    public static class Env
    {
        public static string Locale => Environment.GetEnvironmentVariable("LOCALE") ?? "en-US";
        public static string TimeZone => Environment.GetEnvironmentVariable("TZ") ?? "Europe/Zagreb";
        public static string HeaderUser => Environment.GetEnvironmentVariable(nameof(HeaderUser)) ?? "Remote-User";
        public static string HeaderGroups => Environment.GetEnvironmentVariable(nameof(HeaderGroups)) ?? "Remote-Groups";
        public static string HeaderName => Environment.GetEnvironmentVariable(nameof(HeaderName)) ?? "Remote-Name";
        public static string HeaderEmail => Environment.GetEnvironmentVariable(nameof(HeaderEmail)) ?? "Remote-Email";
        public static string AdminUsers => Environment.GetEnvironmentVariable(nameof(AdminUsers)) ?? string.Empty;
    }
    public static class Routes
    {
        public const string Root = "/";
        public const string Forbidden = "/forbidden";
        public const string Download = "/????";
        public const string DownloadPattern = "/????/{id:guid}";
        public static string DownloadFor(Guid id) => $"{Download}/{id}";
        public const string Attachment = "/????";
        public const string AttachmentPattern = "/????/{id:guid}";
        public static string AttachmentFor(Guid id) => $"{Attachment}/{id}";
        public const string MyFiles = "/????";
        public const string MyFilesPattern = "/????/{*pageRoute}";
        public const string InternalShare = "/????";
        public const string InternalSharePattern = "/????/{shareName?}/{*pageRoute}";
        public const string InternalShareConfiguration = "/internal-share-configuration";
        public const string UsersConfiguration = "/user-configuration";
    }
    public static class Query
    {
        public const string Search = "search";
        public const string Sort = "sort";
        public const string Direction = "dir";
    }
    public static class Paths
    {
        public static string Config => Path.Combine(Environment.CurrentDirectory, "config");
        public static string ConfigFor(string file) => Path.Combine(Config, file);
        public static string Data => Path.Combine(Environment.CurrentDirectory, "data");
        public static string DataFor(string file) => Path.Combine(Data, file);
        public static readonly string AppDbConnectionString = $"Data Source={ConfigFor("app.db")}";
    }
    public static class Config
    {
        static readonly FileInfo file = new(Paths.ConfigFor("configuration.json"));
        static readonly JsonSerializerOptions serializerOptions = new()
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };
        public static Settings Current { get; private set; } = new();
        public static async ValueTask LoadAsync()
        {
            if (file.Exists)
                Current = await LoadFromDiskAsync();
            else
                await SaveToDiskAsync(Current);
        }
        public static async Task<Settings> LoadFromDiskAsync()
        {
            var contents = await File.ReadAllTextAsync(file.FullName);
            var settings = JsonSerializer.Deserialize<Settings>(contents) ?? throw new JsonException("Could not load configuration file");
            return settings;
        }
        public static async ValueTask SaveToDiskAsync(Settings settings)
        {
            var contents = JsonSerializer.Serialize(settings, serializerOptions);
            await File.WriteAllTextAsync(file.FullName, contents);
        }
        public static ValueTask SaveToDiskAsync() => SaveToDiskAsync(Current);
    }
}

public class Settings
{
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool SmtpSsl { get; set; }
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string SmtpFromName { get; set; } = "datotekica";
    public string SmtpFromAddress { get; set; } = "datotekica@example.com";
    public string SmtpSubjectPrefix { get; set; } = "[datotekica] - ";
}