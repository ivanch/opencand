namespace OpenCand.ETL.Contracts
{
    public interface IParserService<CsvObj>
    {
        Task ParseObject(CsvObj record);
    }
}
