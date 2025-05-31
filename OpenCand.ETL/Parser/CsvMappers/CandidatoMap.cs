using CsvHelper.Configuration;
using OpenCand.Parser.Models;
using System.Globalization;

namespace OpenCand.Parser.CsvMappers
{
    public class CandidatoMap : ClassMap<CandidatoCSV>
    {
        public CandidatoMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            // Explicitly handle any special mappings if needed
        }
    }
}
