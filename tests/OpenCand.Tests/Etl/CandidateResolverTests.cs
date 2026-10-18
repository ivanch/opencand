using OpenCand.ETL.Loading;
using OpenCand.Parser.Models;

namespace OpenCand.Tests.Etl;

public class CandidateResolverTests
{
    private static CandidateRecord Record(
        int ano, string sq, string nome, string? cpf = null, DateOnly? dob = null, string nr = "1313", string cargo = "DEPUTADO FEDERAL",
        string uf = "SP", string ue = "SÃO PAULO", string resultado = "ELEITO", string turno = "1", string? apelido = null, string partido = "PT")
        => new(ano, sq, turno, "ESTADUAL", uf, ue, cargo, nr, nome, apelido, cpf, null, dob, "FEMININO", "ADVOGADO", "SOLTEIRO(A)",
            "SUPERIOR COMPLETO", resultado, partido, "PARTIDO " + partido, 13);

    private static readonly DateOnly Birth = new(1970, 3, 5);

    [Fact]
    public void SameCpfInTwoYearsIsOneCandidato()
    {
        var resolver = new CandidateResolver();

        var a = resolver.Add(Record(2018, "1", "MARIA SILVA", "12345678909", Birth));
        var b = resolver.Add(Record(2022, "2", "MARIA SILVA", "12345678909", Birth));

        Assert.Equal(a, b);
        var batch = resolver.TakeBatch();
        Assert.Single(batch.Candidatos);
        Assert.Equal(2, batch.Mappings.Count);
        Assert.Equal(2, batch.Exts.Count);
    }

    [Fact]
    public void SameCpfWithDifferentNamesStillResolvesToTheCpfOwner()
    {
        var resolver = new CandidateResolver();

        var a = resolver.Add(Record(2018, "1", "MARIA SILVA", "12345678909", Birth));
        var b = resolver.Add(Record(2022, "2", "MARIA S. SOUZA", "12345678909", Birth));

        Assert.Equal(a, b);
        var candidato = Assert.Single(resolver.TakeBatch().Candidatos);
        Assert.Equal("MARIA S. SOUZA", candidato.Nome); // newest year wins
    }

    [Fact]
    public void NoCpfIsMatchedByNameAndBirthDate_AndGainsTheCpfLater()
    {
        var resolver = new CandidateResolver();

        var a = resolver.Add(Record(2018, "1", "JOAO SOUZA", null, Birth));
        var b = resolver.Add(Record(2022, "2", "JOAO SOUZA", "11122233396", Birth));

        Assert.Equal(a, b);
        Assert.Equal("11122233396", Assert.Single(resolver.TakeBatch().Candidatos).Cpf);
    }

    [Fact]
    public void DifferentCpfWithTheSameNameAndBirthDateAreDifferentPeople()
    {
        var resolver = new CandidateResolver();

        var a = resolver.Add(Record(2018, "1", "JOSE DA SILVA", "11111111119", Birth, nr: "11"));
        var b = resolver.Add(Record(2018, "2", "JOSE DA SILVA", "22222222228", Birth, nr: "22"));

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void WithoutCpfOrBirthDateTheSameElectionMappingIsTheSameCandidato()
    {
        var resolver = new CandidateResolver();

        var a = resolver.Add(Record(2018, "1", "ANA COSTA", null, null));
        var b = resolver.Add(Record(2018, "1", "ANA COSTA", null, null)); // e.g. the same row in a second file
        var c = resolver.Add(Record(2018, "9", "ANA COSTA", null, null, nr: "9999")); // different number: someone else

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void ResultDoesNotDependOnThreadsOrRepeats_RepeatedRowsAreIdempotent()
    {
        var rows = new[]
        {
            Record(2018, "1", "MARIA SILVA", "12345678909", Birth),
            Record(2018, "2", "JOAO SOUZA", null, Birth.AddDays(1)),
            Record(2022, "3", "MARIA SILVA", "12345678909", Birth, apelido: "MARIA DA URNA"),
            Record(2022, "4", "JOAO SOUZA", "11122233396", Birth.AddDays(1)),
        };

        var first = new CandidateResolver();
        var second = new CandidateResolver();
        foreach (var r in rows) first.Add(r);
        foreach (var r in rows.Concat(rows)) second.Add(r); // every row twice

        var a = first.TakeBatch();
        var b = second.TakeBatch();
        Assert.Equal(a.Candidatos.Count, b.Candidatos.Count);
        Assert.Equal(a.Mappings.Count, b.Mappings.Count);
        Assert.Equal(a.Exts.Count, b.Exts.Count);
        Assert.Equal(2, a.Candidatos.Count);
    }

    [Fact]
    public void SecondRoundIsAnotherMappingOfTheSameCandidato()
    {
        var resolver = new CandidateResolver();

        var a = resolver.Add(Record(2018, "4", "PEDRO LIMA", "98765432100", Birth, resultado: "2º TURNO", cargo: "GOVERNADOR"));
        var b = resolver.Add(Record(2018, "4", "PEDRO LIMA", "98765432100", Birth, resultado: "ELEITO", cargo: "GOVERNADOR", turno: "2"));

        Assert.Equal(a, b);
        Assert.Equal(2, resolver.TakeBatch().Mappings.Count);
    }

    [Fact]
    public void LatestYearWinsForProfileFields_RegardlessOfRowOrder()
    {
        var newestFirst = new CandidateResolver();
        newestFirst.Add(Record(2022, "2", "MARIA SILVA", "12345678909", Birth, apelido: "NOVA"));
        newestFirst.Add(Record(2018, "1", "MARIA SILVA", "12345678909", Birth, apelido: "ANTIGA"));

        var candidato = Assert.Single(newestFirst.TakeBatch().Candidatos);
        Assert.Equal(2022, candidato.UltimoAno);
        Assert.Equal("NOVA", candidato.Apelido);
    }

    [Fact]
    public void ACpfIsNeverReplaced()
    {
        var resolver = new CandidateResolver();
        resolver.Add(Record(2018, "1", "MARIA SILVA", "12345678909", Birth));
        resolver.Add(Record(2022, "2", "MARIA SILVA", null, Birth)); // matched by name + birth date, no CPF in this row

        Assert.Equal("12345678909", Assert.Single(resolver.TakeBatch().Candidatos).Cpf);
    }

    [Fact]
    public void PreloadedCandidatosKeepTheirIds_AndUnchangedRowsAreNotRewritten()
    {
        var id = Guid.NewGuid();
        var resolver = new CandidateResolver();
        resolver.Preload(
            new[] { new ExistingCandidate(id, "12345678909", "MARIA SILVA", Birth, "FEMININO", null, "SÃO PAULO", 2018) },
            new[] { new ExistingMapping(id, "MARIA SILVA", 2018, "DEPUTADO FEDERAL", "SP", "SÃO PAULO", "1313", "ELEITO") });

        var resolved = resolver.Add(Record(2018, "1", "MARIA SILVA", "12345678909", Birth));

        Assert.Equal(id, resolved);
        var batch = resolver.TakeBatch();
        Assert.Empty(batch.Candidatos);   // nothing changed
        Assert.Empty(batch.Mappings);     // the mapping already exists
        Assert.Single(batch.Exts);        // ext rows are always upserted (the SQL skips identical ones)
    }

    [Fact]
    public void TakeBatchResetsThePendingState()
    {
        var resolver = new CandidateResolver();
        resolver.Add(Record(2018, "1", "MARIA SILVA", "12345678909", Birth));
        Assert.Equal(1, resolver.PendingRows);

        Assert.False(resolver.TakeBatch().IsEmpty);
        Assert.Equal(0, resolver.PendingRows);
        Assert.True(resolver.TakeBatch().IsEmpty);
    }

    [Fact]
    public void TryMap_RejectsInvalidNamesAndMissingKeyFields()
    {
        var valid = new CandidatoCSV
        {
            AnoEleicao = 2022, Turno = "1", SiglaUF = "SP", NomeUE = "SÃO PAULO", DescricaoCargo = "VEREADOR", SequencialCandidato = "1",
            NumeroCandidato = "13", NomeCandidato = "MARIA", SiglaPartido = "PT", NomePartido = "PT", NumeroPartido = 13,
            CPFCandidato = "#NULO#", DataNascimento = "#NULO", Genero = "FEMININO", Apelido = "#NULO", Ocupacao = "X", EstadoCivil = "Y", GrauInstrucao = "Z"
        };

        Assert.True(CandidatoLoader.TryMap(valid, out var record, out _));
        Assert.Null(record.Cpf);
        Assert.Null(record.Dob);
        Assert.Null(record.Apelido);
        Assert.Equal("-", record.Resultado);

        valid.NomeCandidato = "#NULO#";
        Assert.False(CandidatoLoader.TryMap(valid, out _, out var reason));
        Assert.Equal("invalid_name", reason);

        valid.NomeCandidato = "MARIA";
        valid.NumeroCandidato = "";
        Assert.False(CandidatoLoader.TryMap(valid, out _, out reason));
        Assert.Equal("missing_field", reason);
    }
}
