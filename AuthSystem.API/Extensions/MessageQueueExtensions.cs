using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.MessageQueue;
using AuthSystem.Domain.Models.MessageQueue.Messages;
using AuthSystem.Infrastructure.MessageQueue.Consumers;
using AuthSystem.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para configurar el sistema de colas de mensajes
    /// </summary>
    public static class MessageQueueExtensions
    {
        /// <summary>
        /// Configura los servicios de cola de mensajes
        /// </summary>
        public static IServiceCollection AddMessageQueueServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar la configuración
            services.Configure<MessageQueueSettings>(configuration.GetSection("MessageQueueSettings"));
            var queueSettings = configuration.GetSection("MessageQueueSettings").Get<MessageQueueSettings>();

            // Registrar servicios
            services.AddScoped<IMessageBusService, MessageBusService>();
            services.AddScoped<IEmailQueueService, EmailQueueService>();
            services.AddScoped<IReportQueueService, ReportQueueService>();
            services.AddScoped<IReportService, ReportService>();

            // Configurar MassTransit con RabbitMQ
            services.AddMassTransit(x =>
            {
                // Registrar consumidores
                x.AddConsumer<EmailMessageConsumer>();
                x.AddConsumer<ReportMessageConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    // Configurar la conexión a RabbitMQ
                    cfg.Host(new Uri($"rabbitmq://{queueSettings.Host}:{queueSettings.Port}/{queueSettings.VirtualHost}"), h =>
                    {
                        h.Username(queueSettings.Username);
                        h.Password(queueSettings.Password);

                        if (queueSettings.UseSsl)
                        {
                            h.UseSsl(s => { });
                        }
                    });

                    // Configurar las colas y los consumidores
                    cfg.ReceiveEndpoint(queueSettings.QueueNames.EmailQueue, e =>
                    {
                        e.PrefetchCount = queueSettings.PrefetchCount;
                        e.ConcurrentMessageLimit = queueSettings.ConcurrentMessageLimit;
                        e.UseMessageRetry(r => r.Interval(queueSettings.RetryCount, TimeSpan.FromMilliseconds(queueSettings.RetryInterval)));
                        e.ConfigureConsumer<EmailMessageConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(queueSettings.QueueNames.ReportQueue, e =>
                    {
                        e.PrefetchCount = queueSettings.PrefetchCount;
                        e.ConcurrentMessageLimit = queueSettings.ConcurrentMessageLimit;
                        e.UseMessageRetry(r => r.Interval(queueSettings.RetryCount, TimeSpan.FromMilliseconds(queueSettings.RetryInterval)));
                        e.ConfigureConsumer<ReportMessageConsumer>(context);
                    });

                    // Configurar la serialización
                    cfg.ConfigureJsonSerializerOptions(options =>
                    {
                        options.PropertyNamingPolicy = null;
                        return options;
                    });
                });
            });

            return services;
        }
    }
}
