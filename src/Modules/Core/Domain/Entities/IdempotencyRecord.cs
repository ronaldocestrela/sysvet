namespace Core.Domain.Entities;

/// <summary>
/// Entidade utilizada para registrar chaves de idempotência processadas, evitando duplicidade.
/// </summary>
public class IdempotencyRecord : Entity
{
    public string Name { get; private set; }

#pragma warning disable CS8618
    protected IdempotencyRecord() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

    private IdempotencyRecord(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static IdempotencyRecord Create(Guid id, string name)
    {
        return new IdempotencyRecord(id, name);
    }
}
