using System.Globalization;
using CsvHelper.Configuration;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Parser.CsvMappers
{
    public class RedeSocialMap : ClassMap<BemCandidatoCSV>
    {
        public RedeSocialMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }
}
