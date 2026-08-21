using System;
using System.Linq;
using System.Threading.Tasks;
using Clients.Infrastructure;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clients.Tests
{
    public class OfflineSyncTests
    {
        private DbContextOptions<OfflineDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<OfflineDbContext>()
                .UseSqlite($"Data Source=offline_sync_test_{Guid.NewGuid()}.db") // Usa um arquivo sqlite isolado para o teste
                .Options;
        }

        [Fact]
        public async Task SaveChangesAsync_WhenWifiOff_ShouldGenerateOutboxMessageForTutor()
        {
            // Arrange - Simula o Wifi Desligado (o cliente apenas salva localmente)
            var options = CreateNewContextOptions();
            
            using (var context = new OfflineDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var tutorId = Guid.NewGuid();
                var email = Email.Create("offline@teste.com").Value;
                var cpf = Cpf.Create("12345678909").Value;
                var phone = Phone.Create("11999999999").Value;
                
                var tutorResult = Tutor.Create("Tutor Offline", email, cpf, phone);
                var tutor = tutorResult.Value;
                
                // Força o ID para simular o cliente gerando
                tutor.GetType().GetProperty("Id")?.SetValue(tutor, tutorId);

                // Act - O usuário clica em salvar sem internet
                context.Tutors.Add(tutor);
                await context.SaveChangesAsync();
            }

            // Assert - Verifica se o OutboxMessage foi criado (O BackgroundWorker processará depois quando a internet voltar)
            using (var context = new OfflineDbContext(options))
            {
                var savedTutor = await context.Tutors.FirstOrDefaultAsync();
                Assert.NotNull(savedTutor);
                Assert.Equal("Tutor Offline", savedTutor.Name);

                var outboxMessages = await context.OutboxMessages.ToListAsync();
                Assert.Single(outboxMessages);
                
                var message = outboxMessages.First();
                Assert.Equal("RegisterTutorCommand", message.Type);
                Assert.Contains("Tutor Offline", message.Payload);
                Assert.Contains("offline@teste.com", message.Payload);
                Assert.Null(message.ProcessedAt); // Ainda não processado
            }

            // Limpa o banco de testes no final
            using (var context = new OfflineDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
            }
        }
    }
}
