using Core.Application.Tutors.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Extensions;

public class SyncMessageDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public static class SyncEndpointExtensions
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/sync").RequireAuthorization().WithTags("Sync");

        // PUSH: Recebe mensagens Outbox do cliente
        group.MapPost("/push", async ([FromBody] List<SyncMessageDto> messages, IMediator mediator) =>
        {
            if (messages == null || !messages.Any())
                return Results.Ok();

            // Lógica Stop-On-First-Error na API para garantir consistência
            foreach (var message in messages.OrderBy(m => m.CreatedAt))
            {
                try
                {
                    object? command = null;
                    if (message.Type == nameof(RegisterTutorCommand))
                    {
                        command = JsonSerializer.Deserialize<RegisterTutorCommand>(message.Payload);
                    }
                    else if (message.Type == nameof(UpdateTutorCommand))
                    {
                        command = JsonSerializer.Deserialize<UpdateTutorCommand>(message.Payload);
                    }
                    // Adicionar outros commands aqui...

                    if (command != null)
                    {
                        // TODO: Tratar o retorno do Mediator (Result<T>) e mapear erros para ProblemDetails/409/400
                        // Isso é um esboço de integração MediatR idempotente
                        await mediator.Send(command);
                    }
                }
                catch (Exception)
                {
                    // Em caso de exceção de concorrência DbUpdateConcurrencyException, retornar 409 Conflict.
                    // Para falhas de validação, retornar 400.
                    // O cliente vai parar de enviar os próximos e retentar.
                    return Results.BadRequest(new { Message = $"Failed to process message {message.Id}" });
                }
            }

            return Results.Ok();
        });

        // PULL: Retorna os dados alterados desde o timestamp fornecido
        group.MapGet("/pull", async ([FromQuery] DateTimeOffset since, Core.Infrastructure.Persistence.CoreDbContext dbContext) =>
        {
            // Consulta de CDC (Change Data Capture) simplificada usando `UpdatedAt`.
            // Para maior robustez (ex: registros deletados), poderíamos usar Soft Delete ou CDC nativo do SQL Server.
            var allTutors = await dbContext.Tutors.ToListAsync();
            var tutors = allTutors
                .Where(t => t.UpdatedAt > since)
                .Select(t => new 
                {
                    Id = t.Id,
                    Name = t.Name,
                    Email = t.Email.Address,
                    Cpf = t.Cpf.Number,
                    Phone = t.Phone.Number,
                    UpdatedAt = t.UpdatedAt,
                    RowVersion = Convert.ToBase64String(t.RowVersion ?? Array.Empty<byte>())
                })
                .ToList();

            var allPets = await dbContext.Pets.ToListAsync();
            var pets = allPets
                .Where(p => p.UpdatedAt > since)
                .Select(p => new 
                {
                    Id = p.Id,
                    Name = p.Name,
                    Species = p.Species.ToString(),
                    Breed = p.Breed,
                    Sex = p.Sex.ToString(),
                    TutorId = p.TutorId,
                    UpdatedAt = p.UpdatedAt,
                    RowVersion = Convert.ToBase64String(p.RowVersion ?? Array.Empty<byte>())
                })
                .ToList();
            
            return Results.Ok(new {
                Tutors = tutors,
                Pets = pets
            });
        });
    }
}
