using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.Services
{
    public class RedeSocialService
    {
        private readonly CandidatoRepository candidatoRepository;
        private readonly RedeSocialRepository redeSocialRepository;

        public RedeSocialService(CandidatoRepository candidatoRepository, RedeSocialRepository redeSocialRepository)
        {
            this.candidatoRepository = candidatoRepository;
            this.redeSocialRepository = redeSocialRepository;
        }

        public async Task AddRedeSocialAsync(RedeSocial redeSocial)
        {
            if (redeSocial == null || string.IsNullOrWhiteSpace(redeSocial.SqCandidato))
            {
                throw new ArgumentNullException(nameof(redeSocial), "RedeSocial cannot be null");
            }

            // Get idCandidato from CandidatoRepository
            var candidato = await candidatoRepository.GetIdCandidatoBySqCandidato(redeSocial.SqCandidato, redeSocial.Ano, redeSocial.SiglaUF);

            if (candidato == null || candidato.IdCandidato == Guid.Empty)
            {
                throw new InvalidOperationException($"AddRedeSocialAsync - Candidato '{redeSocial.SqCandidato}'/{redeSocial.Ano}/'{redeSocial.SiglaUF}' not found.");
            }

            redeSocial.IdCandidato = candidato.IdCandidato;
            redeSocial.Rede = GetRedeSocialType(redeSocial.Link);

            await redeSocialRepository.AddRedeSocialAsync(redeSocial);
        }

        private string GetRedeSocialType(string url)
        {
            switch (url.ToLower())
            {
                case var s when s.Contains("facebook.com"):
                    return "Facebook";
                case var s when s.Contains("twitter.com"):
                case var ss when ss.Contains("x.com"):
                    return "X/Twitter";
                case var s when s.Contains("instagram.com"):
                    return "Instagram";
                case var s when s.Contains("youtube.com"):
                    return "YouTube";
                case var s when s.Contains("linkedin.com"):
                    return "LinkedIn";
                case var s when s.Contains("spotify.com"):
                    return "Spotify";
                case var s when s.Contains("kwai.com"):
                    return "Kwai";
                case var s when s.Contains("tiktok.com"):
                    return "TikTok";
                case var s when s.Contains("threads.com"):
                case var ss when ss.Contains("threads.net"):
                    return "Threads";
                case var s when s.Contains("t.me"):
                case var ss when ss.Contains("telegram.com"):
                    return "Telegram";
                case var s when s.Contains("api.whatsapp"):
                case var ss when ss.Contains("whatsapp.com"):
                case var sss when sss.Contains("wa.me"):
                    return "WhatsApp";
                default:
                    return "Outros";
            }
        }
    }
}
