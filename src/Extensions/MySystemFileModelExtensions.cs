namespace datotekica.Extensions
{
    public static class MySystemFileModelExtensions
    {
        public static IEnumerable<MyDirectoryModel> Filter(this IEnumerable<MyDirectoryModel> source, string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return source;

            return source.Where(m => m.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase));
        }
        public static IEnumerable<MyFileModel> Filter(this IEnumerable<MyFileModel> source, string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return source;

            return source.Where(m => m.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase));
        }
        public static IEnumerable<MyDirectoryModel> Sort(this IEnumerable<MyDirectoryModel> source, string? by, bool desc)
        {
            if (by == nameof(MySystemFileModel.Name))
                return desc ? source.OrderByDescending(m => m.Name) : source.OrderBy(m => m.Name);

            if (by == nameof(MySystemFileModel.Modified))
                return desc ? source.OrderByDescending(m => m.Modified) : source.OrderBy(m => m.Modified);

            return source;
        }
        public static IEnumerable<MyFileModel> Sort(this IEnumerable<MyFileModel> source, string? by, bool desc)
        {
            if (by == nameof(MySystemFileModel.Name))
                return desc ? source.OrderByDescending(m => m.Name) : source.OrderBy(m => m.Name);

            if (by == nameof(MySystemFileModel.Modified))
                return desc ? source.OrderByDescending(m => m.Modified) : source.OrderBy(m => m.Modified);

            if (by == nameof(MyFileModel.Size))
                return desc ? source.OrderByDescending(m => m.Size) : source.OrderBy(m => m.Size);

            return source;
        }
    }
}