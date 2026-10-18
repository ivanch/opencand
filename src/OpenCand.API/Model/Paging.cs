namespace OpenCand.API.Model
{
    /// <summary>Validated ?page=&amp;pageSize= parameters. <see cref="Enabled"/> is false when neither was supplied.</summary>
    public readonly record struct Paging(bool Enabled, int Page, int PageSize)
    {
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 200;

        public long Offset => (long)(Page - 1) * PageSize;

        public static Paging Resolve(int? page, int? pageSize)
        {
            if (page == null && pageSize == null)
                return new Paging(false, 1, 0);

            var resolvedPage = page ?? 1;
            var resolvedSize = pageSize ?? DefaultPageSize;

            if (resolvedPage < 1)
                throw new ArgumentException("Parameter 'page' must be 1 or greater.", nameof(page));
            if (resolvedSize < 1 || resolvedSize > MaxPageSize)
                throw new ArgumentException($"Parameter 'pageSize' must be between 1 and {MaxPageSize}.", nameof(pageSize));

            return new Paging(true, resolvedPage, resolvedSize);
        }
    }
}
