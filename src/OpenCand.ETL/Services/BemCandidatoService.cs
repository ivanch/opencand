using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.Services
{
    public class BemCandidatoService
    {
        private readonly CandidatoRepository candidatoRepository;
        private readonly BemCandidatoRepository bemCandidatoRepository;

        public BemCandidatoService(CandidatoRepository candidatoRepository, BemCandidatoRepository bemCandidatoRepository)
        {
            this.candidatoRepository = candidatoRepository;
            this.bemCandidatoRepository = bemCandidatoRepository;
        }

        public async Task AddBemCandidatoAsync(BemCandidato bemCandidato)
        {
            if (bemCandidato == null || string.IsNullOrWhiteSpace(bemCandidato.SqCandidato))
            {
                throw new ArgumentNullException(nameof(bemCandidato), "BemCandidato cannot be null");
            }

            // Get idCandidato from CandidatoRepository
            var candidato = await candidatoRepository.GetIdCandidatoBySqCandidato(bemCandidato.SqCandidato, bemCandidato.Ano, bemCandidato.SiglaUF, bemCandidato.NomeUE);

            if (candidato == null || candidato.IdCandidato == Guid.Empty)
            {
                throw new InvalidOperationException($"AddBemCandidatoAsync - Candidato '{bemCandidato.SqCandidato}'/{bemCandidato.Ano}/'{bemCandidato.SiglaUF}'/'{bemCandidato.NomeUE}' not found.");
            }

            bemCandidato.IdCandidato = candidato.IdCandidato;

            await bemCandidatoRepository.AddBemCandidatoAsync(bemCandidato);
        }
    }
}
