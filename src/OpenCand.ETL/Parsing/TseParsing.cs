using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenCand.ETL.Parsing
{
    /// <summary>
    /// Pure, culture-independent helpers for the values found in the TSE CSV files.
    /// </summary>
    public static partial class TseParsing
    {
        private static readonly HashSet<string> EmptyMarkers = new(StringComparer.Ordinal)
        {
            "#NE", "#NULO", "#NULO#", "NÃO DIVULGÁVEL", "-4"
        };

        private static readonly string[] DateFormats =
        {
            "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss"
        };

        /// <summary>Trims the value and maps empty strings and the TSE "no data" markers to null.</summary>
        public static string? NullIfTseEmpty(string? value)
        {
            if (value is null) return null;
            var trimmed = value.Trim();
            return trimmed.Length == 0 || EmptyMarkers.Contains(trimmed) ? null : trimmed;
        }

        /// <summary>Trims the value and maps empty strings to null (keeps the TSE markers).</summary>
        public static string? NullIfBlank(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        /// <summary>
        /// Parses a money amount. Empty / marker values give <c>null</c> and succeed; a non-empty value that is not a
        /// number returns false. Accepts "1.234,56" (pt-BR), "1234,56", "1234.56" (a single dot followed by 1-2 digits is a
        /// decimal point) and "1.234" / "1.234.567" (dots are thousands separators).
        /// </summary>
        public static bool TryParseMoney(string? raw, out decimal? value)
        {
            value = null;
            var s = NullIfTseEmpty(raw);
            if (s is null) return true;

            s = s.Replace(" ", "").Replace("R$", "", StringComparison.OrdinalIgnoreCase);

            string normalized;
            if (s.Contains(','))
            {
                normalized = s.Replace(".", "").Replace(',', '.');
            }
            else
            {
                var dot = s.IndexOf('.');
                if (dot >= 0 && dot == s.LastIndexOf('.') && s.Length - dot - 1 is >= 1 and <= 2)
                    normalized = s;
                else
                    normalized = s.Replace(".", "");
            }

            if (decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        /// <summary>Parses a dd/MM/yyyy (or ISO) date or datetime as UTC. Never depends on the current culture.</summary>
        public static DateTime? ParseDateTimeUtc(string? raw)
        {
            var s = NullIfTseEmpty(raw);
            if (s is null) return null;

            return DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
                ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                : null;
        }

        /// <summary>Parses a birth date (dd/MM/yyyy).</summary>
        public static DateOnly? ParseDate(string? raw)
        {
            var s = NullIfTseEmpty(raw);
            if (s is null) return null;

            return DateOnly.TryParseExact(s, new[] { "dd/MM/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed)
                ? parsed
                : null;
        }

        /// <summary>
        /// Returns the 11 digits of a CPF, or null when the value is not a usable CPF (wrong length, masked, or all
        /// digits equal such as 00000000000, which would merge unrelated people). Some files write the CPF as a number
        /// and drop its leading zeros: a shorter value is zero-padded when that gives valid check digits.
        /// </summary>
        public static string? ParseCpf(string? raw)
        {
            var digits = OnlyDigits(NullIfTseEmpty(raw));
            if (digits is null || digits.Length > 11) return null;
            if (digits.Length < 11)
            {
                digits = digits.PadLeft(11, '0');
                if (!HasValidCpfCheckDigits(digits)) return null;
            }
            if (digits.All(c => c == digits[0])) return null;
            return digits;
        }

        internal static bool HasValidCpfCheckDigits(string cpf)
        {
            for (var n = 9; n <= 10; n++)
            {
                var sum = 0;
                for (var i = 0; i < n; i++) sum += (cpf[i] - '0') * (n + 1 - i);
                if (sum * 10 % 11 % 10 != cpf[n] - '0') return false;
            }
            return true;
        }

        /// <summary>Splits a CPF/CNPJ document: 11 digits are a CPF, 14 digits a CNPJ, anything else neither.</summary>
        public static (string? Cpf, string? Cnpj) SplitCpfCnpj(string? raw)
        {
            var digits = OnlyDigits(NullIfTseEmpty(raw));
            return digits?.Length switch
            {
                11 => (digits, null),
                14 => (null, digits),
                _ => (null, null)
            };
        }

        private static string? OnlyDigits(string? value)
        {
            if (value is null) return null;
            var digits = new string(value.Where(char.IsAsciiDigit).ToArray());
            return digits.Length == 0 ? null : digits;
        }

        /// <summary>
        /// Returns the link with an https:// scheme when it has none, or null when it is not a usable http(s) URL
        /// (other schemes such as javascript:/mailto:, user info, hosts without a dot).
        /// </summary>
        public static string? NormalizeUrl(string? raw)
        {
            var s = NullIfTseEmpty(raw);
            if (s is null || s.Any(char.IsWhiteSpace)) return null;

            if (!s.Contains("://", StringComparison.Ordinal))
            {
                // "mailto:x@y", "javascript:alert(1)": a scheme without "//" whose name is not a host (hosts have dots)
                var scheme = SchemeWithoutSlashes().Match(s);
                if (scheme.Success && !scheme.Groups["name"].Value.Contains('.')) return null;

                s = "https://" + s.TrimStart('/');
            }

            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return null;
            if (!string.IsNullOrEmpty(uri.UserInfo)) return null;
            if (string.IsNullOrEmpty(uri.Host) || !uri.Host.Contains('.')) return null;

            return s;
        }

        private static readonly (string Domain, string Rede)[] RedeDomains =
        {
            ("facebook.com", "Facebook"),
            ("fb.com", "Facebook"),
            ("twitter.com", "X/Twitter"),
            ("x.com", "X/Twitter"),
            ("instagram.com", "Instagram"),
            ("youtube.com", "YouTube"),
            ("youtu.be", "YouTube"),
            ("linkedin.com", "LinkedIn"),
            ("spotify.com", "Spotify"),
            ("kwai.com", "Kwai"),
            ("tiktok.com", "TikTok"),
            ("threads.com", "Threads"),
            ("threads.net", "Threads"),
            ("t.me", "Telegram"),
            ("telegram.me", "Telegram"),
            ("telegram.com", "Telegram"),
            ("whatsapp.com", "WhatsApp"),
            ("wa.me", "WhatsApp"),
        };

        /// <summary>Classifies a normalized URL by its host (exact domain or subdomain), "Outros" when unknown.</summary>
        public static string ClassifyRede(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "Outros";

            var host = uri.Host.ToLowerInvariant();
            foreach (var (domain, rede) in RedeDomains)
            {
                if (host == domain || host.EndsWith("." + domain, StringComparison.Ordinal))
                    return rede;
            }

            return "Outros";
        }

        [GeneratedRegex(@"^(?<name>[A-Za-z][A-Za-z0-9+.\-]*):(?!//)")]
        private static partial Regex SchemeWithoutSlashes();
    }
}
