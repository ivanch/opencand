using CsvHelper.Configuration;
using OpenCand.Parser.Models;
using System.Globalization;

namespace OpenCand.Parser.CsvMappers
{
    public class BemCandidatoMap : ClassMap<BemCandidatoCSV>
    {
        public BemCandidatoMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            // Explicitly handle any special mappings if needed
        }
    }
}
