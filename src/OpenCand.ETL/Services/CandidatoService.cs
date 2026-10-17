using OpenCand.Core.Models;
using OpenCand.ETL.Repository;
using OpenCand.Repository;

namespace OpenCand.Services
{
    public class CandidatoService
    {
        private readonly CandidatoRepository candidatoRepository;
        private readonly PartidoRepository partidoRepository;

        public CandidatoService(CandidatoRepository candidatoRepository, PartidoRepository partidoRepository)
        {
            this.candidatoRepository = candidatoRepository;
            this.partidoRepository = partidoRepository;
        }

        public async Task AddCandidatoAsync(Candidato candidato)
        {
            if (candidato == null || candidato.CandidatoExt == null || candidato.Eleicoes == null)
            {
                throw new ArgumentNullException(nameof(candidato), "Candidato cannot be null");
            }

            if (candidato.Eleicoes == null || candidato.Eleicoes.Count == 0)
            {
                throw new ArgumentException("Candidato must have at least one mapping", nameof(candidato));
            }

            var candidatoMapping = candidato.Eleicoes.First();
            var candidatoExt = candidato.CandidatoExt.First();

            // Add partido data
            if (candidatoMapping.Partido != null)
            {
                await partidoRepository.AddPartidoAsync(candidatoMapping.Partido);
            }

            // Check if the candidate already exists in the database
            Candidato? existingCandidato;
            if (candidato.Cpf == null || candidato.Cpf.Length != 11)
            {
                existingCandidato = await candidatoRepository.GetCandidatoByNome(candidato.Nome, candidato.DataNascimento.GetValueOrDefault());
            }
            else
            {
                existingCandidato = await candidatoRepository.GetCandidatoByCpf(candidato.Cpf);
            }

            // If the candidate already exists, we can update the mappings
            if (existingCandidato != null)
            {
                candidato.IdCandidato = existingCandidato.IdCandidato;
                candidato.Cpf = GetNonEmptyString(existingCandidato.Cpf, candidato.Cpf);
                candidato.Email = GetNonEmptyString(existingCandidato.Email, candidato.Email);
                candidato.Apelido = GetNonEmptyString(existingCandidato.Apelido, candidato.Apelido);
                candidato.Sexo = GetNonEmptyString(existingCandidato.Sexo, candidato.Sexo);

                candidatoMapping.IdCandidato = candidato.IdCandidato;
                candidatoMapping.Cpf = GetNonEmptyString(candidato.Cpf, candidatoMapping.Cpf);

                candidatoExt.IdCandidato = candidato.IdCandidato;

                // Update the entries for the existing candidate
                await candidatoRepository.AddCandidatoAsync(candidato);
                await candidatoRepository.AddCandidatoMappingAsync(candidatoMapping);
                await candidatoRepository.AddCandidatoExtAsync(candidatoExt);
                return;
            }

            // If the candidate does not exist, we create a new one
            CandidatoMapping? existingMapping = await candidatoRepository.GetCandidatoMappingByDetails(candidato.Nome,
                                                                                                       candidatoMapping.Ano,
                                                                                                       candidatoMapping.Cargo,
                                                                                                       candidatoMapping.SiglaUF,
                                                                                                       candidatoMapping.NomeUE,
                                                                                                       candidatoMapping.NrCandidato,
                                                                                                       candidatoMapping.Resultado);
            // Check if exists
            if (existingMapping != null)
            {
                candidato.IdCandidato = existingMapping.IdCandidato;
                candidatoExt.IdCandidato = existingMapping.IdCandidato;

                candidato.Cpf = GetNonEmptyString(existingMapping.Cpf, candidato.Cpf);

                await candidatoRepository.AddCandidatoAsync(candidato);
                await candidatoRepository.AddCandidatoExtAsync(candidatoExt);
                return;
            }

            // No current mapping, we create a new one and create a new mapping for the current election
            candidato.IdCandidato = Guid.NewGuid();

            // Set the mapping properties
            candidatoMapping.IdCandidato = candidato.IdCandidato;
            candidatoMapping.Cpf = candidato.Cpf;
            candidatoMapping.Nome = candidato.Nome;

            candidatoExt.IdCandidato = candidato.IdCandidato;

            await candidatoRepository.AddCandidatoAsync(candidato);
            await candidatoRepository.AddCandidatoMappingAsync(candidatoMapping);
            await candidatoRepository.AddCandidatoExtAsync(candidatoExt);
        }

        public string GetNonEmptyString(string? value1, string? value2)
        {
            if (!string.IsNullOrWhiteSpace(value1))
            {
                return value1;
            }
            else if (!string.IsNullOrWhiteSpace(value2))
            {
                return value2;
            }
            return string.Empty;
        }

    }
}
