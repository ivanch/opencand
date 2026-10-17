namespace OpenCand.Core.Utils
{
    public static class CpfMasking
    {
        /// <summary>
        /// Masks a CPF number by replacing the middle 3 digits with '*'
        /// </summary>
        /// <param name="cpf">The CPF number to mask.</param>
        /// <returns>The masked CPF number.</returns>
        public static string MaskCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
            {
                return cpf;
            }

            // Mask the middle 3 digits
            return $"{cpf.Substring(0, 3)}***{cpf.Substring(6)}";
        }
    }
}
