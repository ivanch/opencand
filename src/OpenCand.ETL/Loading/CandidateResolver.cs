namespace OpenCand.ETL.Loading
{
    public sealed record CandidateRecord(
        int Ano, string Sq, string Turno, string? TipoEleicao, string SiglaUf, string NomeUe, string Cargo,
        string NrCandidato, string Nome, string? Apelido, string? Cpf, string? Email, DateOnly? Dob, string? Sexo,
        string? Ocupacao, string? EstadoCivil, string? Escolaridade, string Resultado,
        string SiglaPartido, string NomePartido, int NumeroPartido);

    public sealed record ExistingCandidate(Guid Id, string? Cpf, string Nome, DateOnly? Dob, string? Sexo, string? Apelido, string? Localidade, int UltimoAno);

    public sealed record ExistingMapping(Guid Id, string Nome, int Ano, string Cargo, string SiglaUf, string NomeUe, string NrCandidato, string Resultado);

    public sealed record CandidatoRow(Guid Id, string? Cpf, string Nome, DateOnly? Dob, string? Sexo, string? Apelido, string? Localidade, int UltimoAno);

    public sealed record MappingRow(Guid Id, string? Cpf, string Nome, string Sq, string Turno, int Ano, string? TipoEleicao, string SiglaUf,
        string NomeUe, string Cargo, string SiglaPartido, string NrCandidato, string Resultado);

    public sealed record ExtRow(Guid Id, int Ano, string? Apelido, string? Email, string? EstadoCivil, string? Escolaridade, string? Ocupacao);

    public sealed record PartidoRow(string Sigla, string Nome, int Numero);

    public sealed class CandidateBatch
    {
        public List<CandidatoRow> Candidatos { get; } = new();
        public List<MappingRow> Mappings { get; } = new();
        public List<ExtRow> Exts { get; } = new();
        public List<PartidoRow> Partidos { get; } = new();

        public bool IsEmpty => Candidatos.Count == 0 && Mappings.Count == 0 && Exts.Count == 0 && Partidos.Count == 0;
    }

    /// <summary>
    /// Decides which candidato each CSV row belongs to. Runs single-threaded and fully in memory, so the outcome does not
    /// depend on thread timing (the old per-row parallel lookups created duplicate candidates for the same CPF).
    ///
    /// Resolution order: CPF; name + birth date (when the row has no CPF, or the match has none yet); the exact same
    /// election mapping. Anything else is a new candidato. Rows must be added in ascending election year: the newest year
    /// wins for name, birth date, location, urna name and sex; the CPF, once known, is never replaced.
    /// </summary>
    public sealed class CandidateResolver
    {
        private readonly record struct MappingKey(string Nome, int Ano, string Cargo, string SiglaUf, string NomeUe, string NrCandidato, string Resultado);
        private readonly record struct MappingPk(Guid Id, int Ano, string SiglaUf, string NomeUe, string Cargo, string NrCandidato, string Resultado);

        private sealed class State
        {
            public Guid Id;
            public string? Cpf;
            public string Nome = string.Empty;
            public DateOnly? Dob;
            public string? Sexo;
            public string? Apelido;
            public string? Localidade;
            public int UltimoAno;
        }

        private readonly Dictionary<string, Guid> cpfToId = new();
        private readonly Dictionary<(string Nome, DateOnly Dob), Guid> nomeDobToId = new();
        private readonly Dictionary<MappingKey, Guid> mappingToId = new();
        private readonly Dictionary<Guid, State> states = new();

        private readonly HashSet<Guid> dirty = new();
        private readonly HashSet<MappingPk> pendingMappingKeys = new();
        private readonly List<MappingRow> pendingMappings = new();
        private readonly Dictionary<(Guid Id, int Ano), ExtRow> pendingExts = new();
        private readonly Dictionary<string, PartidoRow> pendingPartidos = new();

        public int CandidatoCount => states.Count;

        public int PendingRows { get; private set; }

        public void Preload(IEnumerable<ExistingCandidate> candidatos, IEnumerable<ExistingMapping> mappings)
        {
            foreach (var c in candidatos)
            {
                states[c.Id] = new State
                {
                    Id = c.Id, Cpf = c.Cpf, Nome = c.Nome, Dob = c.Dob, Sexo = c.Sexo, Apelido = c.Apelido,
                    Localidade = c.Localidade, UltimoAno = c.UltimoAno
                };
                if (c.Cpf is not null) cpfToId.TryAdd(c.Cpf, c.Id);
                if (c.Dob is not null) nomeDobToId.TryAdd((c.Nome, c.Dob.Value), c.Id);
            }

            foreach (var m in mappings)
            {
                mappingToId.TryAdd(new MappingKey(m.Nome, m.Ano, m.Cargo, m.SiglaUf, m.NomeUe, m.NrCandidato, m.Resultado), m.Id);
            }
        }

        public Guid Add(CandidateRecord r)
        {
            var key = new MappingKey(r.Nome, r.Ano, r.Cargo, r.SiglaUf, r.NomeUe, r.NrCandidato, r.Resultado);
            var state = Find(r, key);
            var isNew = state is null;

            if (state is null)
            {
                state = new State { Id = Guid.NewGuid(), Nome = r.Nome };
                states[state.Id] = state;
            }

            var changed = Merge(state, r) || isNew;
            if (changed) dirty.Add(state.Id);

            if (state.Cpf is not null) cpfToId.TryAdd(state.Cpf, state.Id);
            if (r.Dob is not null) nomeDobToId.TryAdd((r.Nome, r.Dob.Value), state.Id);
            if (state.Dob is not null) nomeDobToId.TryAdd((state.Nome, state.Dob.Value), state.Id);

            // a mapping with the same key that already points at this candidato exists in the database / this batch
            if (!(mappingToId.TryGetValue(key, out var mappedId) && mappedId == state.Id))
            {
                mappingToId.TryAdd(key, state.Id);
                var pk = new MappingPk(state.Id, r.Ano, r.SiglaUf, r.NomeUe, r.Cargo, r.NrCandidato, r.Resultado);
                if (pendingMappingKeys.Add(pk))
                {
                    pendingMappings.Add(new MappingRow(state.Id, state.Cpf, r.Nome, r.Sq, r.Turno, r.Ano, r.TipoEleicao, r.SiglaUf,
                        r.NomeUe, r.Cargo, r.SiglaPartido, r.NrCandidato, r.Resultado));
                }
            }

            pendingExts[(state.Id, r.Ano)] = new ExtRow(state.Id, r.Ano, r.Apelido, r.Email, r.EstadoCivil, r.Escolaridade, r.Ocupacao);
            pendingPartidos[r.SiglaPartido] = new PartidoRow(r.SiglaPartido, r.NomePartido, r.NumeroPartido);

            PendingRows++;
            return state.Id;
        }

        /// <summary>Returns everything added since the last call and resets the pending state.</summary>
        public CandidateBatch TakeBatch()
        {
            var batch = new CandidateBatch();
            foreach (var id in dirty)
            {
                var s = states[id];
                batch.Candidatos.Add(new CandidatoRow(s.Id, s.Cpf, s.Nome, s.Dob, s.Sexo, s.Apelido, s.Localidade, s.UltimoAno));
            }

            batch.Mappings.AddRange(pendingMappings);
            batch.Exts.AddRange(pendingExts.Values);
            batch.Partidos.AddRange(pendingPartidos.Values);

            dirty.Clear();
            pendingMappingKeys.Clear();
            pendingMappings.Clear();
            pendingExts.Clear();
            pendingPartidos.Clear();
            PendingRows = 0;
            return batch;
        }

        private State? Find(CandidateRecord r, MappingKey key)
        {
            if (r.Cpf is not null && cpfToId.TryGetValue(r.Cpf, out var byCpf))
                return states[byCpf];

            if (r.Dob is not null && nomeDobToId.TryGetValue((r.Nome, r.Dob.Value), out var byName))
            {
                var candidate = states[byName];
                // a different CPF means a different person with the same name and birth date
                if (r.Cpf is null || candidate.Cpf is null) return candidate;
            }

            if (mappingToId.TryGetValue(key, out var byMapping))
            {
                var candidate = states[byMapping];
                if (r.Cpf is null || candidate.Cpf is null || candidate.Cpf == r.Cpf) return candidate;
            }

            return null;
        }

        private static bool Merge(State s, CandidateRecord r)
        {
            var before = (s.Cpf, s.Nome, s.Dob, s.Sexo, s.Apelido, s.Localidade, s.UltimoAno);

            if (s.Cpf is null && r.Cpf is not null) s.Cpf = r.Cpf;

            if (r.Ano >= s.UltimoAno)
            {
                s.Nome = r.Nome;
                s.UltimoAno = r.Ano;
                s.Localidade = r.NomeUe;
                s.Dob = r.Dob ?? s.Dob;
                s.Sexo = r.Sexo ?? s.Sexo;
                s.Apelido = r.Apelido ?? s.Apelido;
            }
            else
            {
                s.Dob ??= r.Dob;
                s.Sexo ??= r.Sexo;
                s.Apelido ??= r.Apelido;
            }

            return before != (s.Cpf, s.Nome, s.Dob, s.Sexo, s.Apelido, s.Localidade, s.UltimoAno);
        }
    }
}
