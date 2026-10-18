namespace OpenCand.Core.Utils
{
    public static class CpfMasking
    {
        /// <summary>
        /// Masks an 11-digit CPF, keeping only the 6 middle digits: 12345678901 becomes ***.456.789-**
        /// </summary>
        /// <param name="cpf">The CPF number to mask.</param>
        /// <returns>The masked CPF number, or the input unchanged when it is not an 11-character value.</returns>
        public static string MaskCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
            {
                return cpf;
            }

            return $"***.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-**";
        }
    }
}
