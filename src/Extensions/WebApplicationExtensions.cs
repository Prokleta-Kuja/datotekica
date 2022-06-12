using System.IO.Compression;
using datotekica.Services;
using Microsoft.AspNetCore.StaticFiles;

namespace datotekica.Extensions;

public static class WebApplicationExtensions
{
    public static void MapDownload(this WebApplication app)
    {
        app.MapGet(C.Routes.DownloadPattern, async (Guid id, CacheService cache, HttpContext ctx) =>
        {
            if (!cache.TryGetDownload(id, out var items) || !items.Any())
            {
                ctx.Response.StatusCode = StatusCodes.Status410Gone;
                await ctx.Response.WriteAsync("This download has expired");
                return;
            }

            var ctProvider = new FileExtensionContentTypeProvider();
            if (items.Count == 1 && !items.First().Value)
            {
                var fileItem = items.First();
                var file = new FileInfo(fileItem.Key);
                if (!file.Exists)
                {
                    ctx.Response.StatusCode = StatusCodes.Status410Gone;
                    await ctx.Response.WriteAsync("This download has expired");
                    return;
                }

                // ctx.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{file.Name}\"");
                ctx.Response.ContentLength = file.Length;
                if (ctProvider.TryGetContentType(file.FullName, out var ct))
                    ctx.Response.ContentType = ct;

                try { await ctx.Response.SendFileAsync(file.FullName, ctx.RequestAborted); }
                catch (OperationCanceledException) { }
                return;
            }

            // TODO: try calculate total size
            ctx.Response.ContentType = ctProvider.Mappings[".zip"];
            ctx.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"datotekica-{DateTime.UtcNow.Ticks}.zip\"");
            using var zip = new ZipArchive(ctx.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);
            foreach (var item in items)
            {
                if (item.Value)
                {
                    var dir = new DirectoryInfo(item.Key);
                    await AddDirectory(zip, dir, string.Empty);
                }
                else
                {
                    var file = new FileInfo(item.Key);
                    await AddFile(zip, file, string.Empty);
                }
            }
        });
        static async Task AddDirectory(ZipArchive zip, DirectoryInfo dir, string? prefix)
        {
            if (!dir.Exists)
                return;

            if (string.IsNullOrWhiteSpace(prefix))
                prefix = $"{dir.Name}/";
            else
                prefix = $"{prefix}{dir.Name}/";

            foreach (var sub in dir.EnumerateDirectories())
                await AddDirectory(zip, sub, prefix);

            foreach (var file in dir.EnumerateFiles())
                await AddFile(zip, file, prefix);
        }
        static async Task AddFile(ZipArchive zip, FileInfo file, string? prefix)
        {
            if (!file.Exists)
                return;

            var entry = zip.CreateEntry($"{prefix}{file.Name}");
            entry.ExternalAttributes = entry.ExternalAttributes | (Convert.ToInt32("664", 8) << 16);
            using var entryStream = entry.Open();
            using var fileStream = file.OpenRead();
            await fileStream.CopyToAsync(entryStream);
        }
    }
}