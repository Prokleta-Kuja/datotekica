using System.Text.Json;

namespace datotekica;

public static class C
{
    public static class Env
    {
        public static string Locale => Environment.GetEnvironmentVariable("LOCALE") ?? "en-US";
        public static string TimeZone => Environment.GetEnvironmentVariable("TZ") ?? "Europe/Zagreb";
    }
    public static class Routes
    {
        public const string Root = "/";
        public const string MyFiles = "/📁";
    }
    public static class Paths
    {
        public static string AppData => Path.Combine(Environment.CurrentDirectory, "data");
        public static string AppDataFor(string file) => Path.Combine(AppData, file);
        public static readonly string AppDbConnectionString = $"Data Source={AppDataFor("app.db")}";
    }
    public static class Config
    {
        static readonly FileInfo file = new(Paths.AppDataFor("configuration.json"));
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