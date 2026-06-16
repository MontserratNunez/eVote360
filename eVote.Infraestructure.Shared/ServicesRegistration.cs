using eVote360.Core.Application.EventHandlers;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Events;
using eVote360.Infraestructure.Shared.Events;
using eVote360.Infraestructure.Shared.OCR;
using Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eVote360.Infraestructure.Shared
{
    public static class ServicesRegistration
    {
        public static void AddInfraestructureLayerIoc(this IServiceCollection services)
        {
            #region Repositories IOC
            services.AddTransient<IOcrService, TesseractOcrService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IEventHandler<ConfirmationCodeEvent>, NotificationHandler>();
            services.AddScoped<IEventHandler<VoteSummaryEvent>, NotificationHandler>();
            #endregion
        }
    }
}
