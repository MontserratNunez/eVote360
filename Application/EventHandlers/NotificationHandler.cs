using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Events;
using System.Text;

namespace eVote360.Core.Application.EventHandlers
{
    public class NotificationHandler : IEventHandler<ConfirmationCodeEvent>, IEventHandler<VoteSummaryEvent>
    {
        private readonly IEmailService _emailService;

        public NotificationHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(ConfirmationCodeEvent e)
        {
            string htmlBody = $@"
                <h2>Hola {e.Name},</h2>
                <p>Su código de verificación para continuar con el proceso de votación es:</p>
                <div style='text-align: center; margin: 20px; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #2c3e50;'>
                    {e.Code}
                </div>
                <p>Este código tendrá una vigencia de 5 minutos.</p>
                <p>Si usted no inició este proceso, ignore este mensaje.</p>";

            await _emailService.SendEmail(e.Email, "Código de verificación para votar", htmlBody);
        }

        public async Task HandleAsync(VoteSummaryEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.CitizenEmail)) return;

            var summaryBuilder = new StringBuilder();
            foreach (var line in e.VotedPositions)
            {
                summaryBuilder.Append($"<p><strong>Puesto:</strong> {line.PositionName}<br/>");
                summaryBuilder.Append($"<strong>Selección:</strong> {line.Selection}<br/>");
                if (!string.IsNullOrEmpty(line.PoliticalParty))
                {
                    summaryBuilder.Append($"<strong>Partido:</strong> {line.PoliticalParty}<br/>");
                }
                summaryBuilder.Append("</p><hr/>");
            }

            string htmlBody = $@"
                <h2>Hola {e.CitizenName},</h2>
                <p>Su proceso de votación ha sido completado correctamente.</p>
                <h3>Resumen de selección:</h3>
                <p><strong>Elección:</strong> {e.ElectionName}</p>
                <p><strong>Fecha de la elección:</strong> {e.ElectionDate}</p>
                {summaryBuilder}
                <p>Gracias por ejercer su derecho al voto.</p>";

            await _emailService.SendEmail(e.CitizenEmail, "Resumen de su participación electoral", htmlBody);
        }
    }
}
