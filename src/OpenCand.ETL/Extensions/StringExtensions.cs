namespace OpenCand.ETL.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value) ||
                   value == "#NE" ||
                   value == "#NULO" ||
                   value == "#NULO#" ||
                   value == "NÃO DIVULGÁVEL" ||
                   value == "-4";
        }
    }
}
