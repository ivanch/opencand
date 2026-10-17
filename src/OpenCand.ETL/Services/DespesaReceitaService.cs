using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.ETL.Services
{
    public class DespesaReceitaService
    {
        private readonly DespesaReceitaRepository despesaReceitaRepository;
        private readonly CandidatoRepository candidatorepository;

        public DespesaReceitaService(DespesaReceitaRepository despesaReceitaRepository, CandidatoRepository candidatoRepository)
        {
            this.despesaReceitaRepository = despesaReceitaRepository;
            this.candidatorepository = candidatoRepository;
        }

        public async Task AddDespesaAsync(Despesa despesa)
        {
            if (despesa == null || string.IsNullOrEmpty(despesa.SqCandidato))
            {
                throw new ArgumentNullException(nameof(despesa), "Despesa cannot be null");
            }

            var idCandidato = await candidatorepository.GetIdCandidatoBySqCandidato(despesa.SqCandidato);
            if (idCandidato == Guid.Empty)
            {
                throw new ArgumentException($"Candidato with SqCandidato {despesa.SqCandidato} not found.");
            }

            despesa.IdCandidato = (Guid)idCandidato;

            await despesaReceitaRepository.AddDespesaAsync(despesa);
        }

        public async Task AddReceitaAsync(Receita receita)
        {
            if (receita == null || string.IsNullOrEmpty(receita.SqCandidato))
            {
                throw new ArgumentNullException(nameof(receita), "Receita cannot be null");
            }

            var idCandidato = await candidatorepository.GetIdCandidatoBySqCandidato(receita.SqCandidato);
            if (idCandidato == Guid.Empty)
            {
                throw new ArgumentException($"Candidato with SqCandidato {receita.SqCandidato} not found.");
            }

            receita.IdCandidato = (Guid)idCandidato;

            await despesaReceitaRepository.AddReceitaAsync(receita);
        }

        public async Task DeleteDespesaAsync()
        {
            await despesaReceitaRepository.DeleteDespesaAsync();
        }

        public async Task DeleteReceitaAsync()
        {
            await despesaReceitaRepository.DeleteReceitaAsync();
        }

    }
}
