using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.Services
{
    public class CandidatoService
    {
        private readonly CandidatoRepository candidatoRepository;

        public CandidatoService(CandidatoRepository candidatoRepository)
        {
            this.candidatoRepository = candidatoRepository;
        }

        public async Task AddCandidatoAsync(Candidato candidato)
        {
            if (candidato == null)
            {
                throw new ArgumentNullException(nameof(candidato), "Candidato cannot be null");
            }

            if (candidato.Eleicoes == null || candidato.Eleicoes.Count == 0)
            {
                throw new ArgumentException("Candidato must have at least one mapping", nameof(candidato));
            }

            var candidatoMapping = candidato.Eleicoes.First();

            List<CandidatoMapping>? mappings = null;
            CandidatoMapping? existingMapping = null;
            if (candidato.Cpf == null || candidato.Cpf.Length != 11)
            {
                mappings = await candidatoRepository.GetCandidatoMappingByNome(candidato.Nome);
            }
            else
            {
                mappings = await candidatoRepository.GetCandidatoMappingByCpf(candidato.Cpf);
            }

            // Check if exists
            if (mappings != null && mappings.Count > 0)
            {
                existingMapping = mappings.FirstOrDefault(m => m.Ano == candidatoMapping.Ano &&
                                                               m.Cargo == candidatoMapping.Cargo &&
                                                               m.SiglaUF == candidatoMapping.SiglaUF &&
                                                               m.NomeUE == candidatoMapping.NomeUE &&
                                                               m.NrCandidato == candidatoMapping.NrCandidato &&
                                                               m.Resultado == candidatoMapping.Resultado);
                
                // Already exists one for the current election
                if (existingMapping != null)
                {
                    candidato.IdCandidato = existingMapping.IdCandidato;
                    candidato.Cpf = existingMapping.Cpf;

                    await candidatoRepository.AddCandidatoAsync(candidato);
                    return;
                }
                // If exists (but not for the current election), we take the existing idcandidato
                // and create a new mapping for the current election
                else
                {
                    existingMapping = mappings.First();
                    candidato.IdCandidato = existingMapping.IdCandidato;
                    candidato.Cpf = existingMapping.Cpf;
                }
            }
            else
            {
                // No current mapping, we create a new one
                // and create a new mapping for the current election
                candidato.IdCandidato = Guid.NewGuid();
            }

            // Set the mapping properties
            candidatoMapping.IdCandidato = candidato.IdCandidato;
            candidatoMapping.Cpf = candidato.Cpf;
            candidatoMapping.Nome = candidato.Nome;

            await candidatoRepository.AddCandidatoAsync(candidato);
            await candidatoRepository.AddCandidatoMappingAsync(candidatoMapping);
        }

    }
}
