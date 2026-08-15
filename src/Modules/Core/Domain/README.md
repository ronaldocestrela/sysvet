# `src/Modules/Core/Domain/` — Camada de Domínio do Módulo Core

Camada mais interna da Clean Architecture. **Não possui dependências externas** — apenas .NET BCL puro. Contém as regras de negócio puras, entidades, Value Objects e abstrações de repositório.

## Estrutura de Arquivos

```
Domain/
├── Entity.cs                    ← Base abstrata para entidades e aggregate roots
├── Result.cs                    ← Padrão Result<T> e tipo Error
├── Entities/
│   ├── Tutor.cs                 ← Aggregate Root principal do sistema
│   ├── Pet.cs                   ← Entidade de domínio do animal
│   └── PetEnums.cs              ← Enumerações PetSpecies e PetSex
└── ValueObjects/
    ├── Cpf.cs                   ← CPF brasileiro validado
    ├── Email.cs                 ← E-mail validado
    └── Phone.cs                 ← Telefone brasileiro validado
```

## Detalhamento dos Arquivos

### [`Entity.cs`](./Entity.cs)
Define dois tipos base essenciais do DDD:

- **`Entity`**: Classe abstrata com `Id` (Guid), igualdade estrutural por Id e operadores `==`/`!=`. Todo objeto de domínio identificável herda desta classe.
- **`AggregateRoot`**: Estende `Entity`. Marca a raiz de um agregado DDD — o único ponto de entrada para modificar o agregado. Futuramente abrigará a lista de eventos de domínio (`IDomainEvent`).

### [`Result.cs`](./Result.cs)
Implementação do **padrão Result** para eliminar exceções no fluxo normal de negócio:

- **`Error(Code, Message)`**: Record imutável que representa um erro com código estruturado (ex: `"Tutor.InvalidName"`) e mensagem legível.
- **`Result`**: Encapsula sucesso/falha sem valor. Propriedades: `IsSuccess`, `IsFailure`, `Error`. Métodos de fábrica: `Success()`, `Failure(error)`.
- **`Result<TValue>`**: Estende `Result` com um valor de retorno. `Value` lança `InvalidOperationException` se acessado em caso de falha. Suporta conversão implícita a partir de `TValue`.

### [`Entities/Tutor.cs`](./Entities/Tutor.cs)
**Aggregate Root** que representa o dono/responsável pelo pet:
- Propriedades: `Name`, `Email` (VO), `Cpf` (VO), `Phone` (VO), `Pets` (coleção imutável)
- Factory Method estático `Create(...)` com validações: nome mínimo de 2 chars, e-mail/CPF/telefone obrigatórios
- Método `AddPet(Pet)`: adiciona um pet à coleção interna com validação de nulidade

### [`Entities/Pet.cs`](./Entities/Pet.cs)
**Entidade de domínio** que representa o animal atendido:
- Propriedades: `Name`, `Species` (enum), `Breed`, `Sex` (enum), `TutorId` (FK para `Tutor`)
- Factory Method `Create(...)` valida: nome não vazio, `TutorId` não é `Guid.Empty`
- Construtor privado — só pode ser criado via `Pet.Create()`

### [`Entities/PetEnums.cs`](./Entities/PetEnums.cs)
Enumerações do domínio animal:
- **`PetSpecies`**: `Dog=1`, `Cat=2`, `Bird=3`, `Reptile=4`, `Other=99`
- **`PetSex`**: `Male=1`, `Female=2`, `Unknown=3`

### [`ValueObjects/Cpf.cs`](./ValueObjects/Cpf.cs)
Value Object para **CPF brasileiro**:
- Aceita CPF com ou sem formatação (`000.000.000-00` ou `00000000000`)
- Valida: não vazio, exatamente 11 dígitos, não todos os dígitos iguais, dígitos verificadores corretos (algoritmo Módulo 11)
- `ToString()` formata como `000.000.000-00`
- Armazena apenas os 11 dígitos numéricos em `Number`

### [`ValueObjects/Email.cs`](./ValueObjects/Email.cs)
Value Object para **endereço de e-mail**:
- Valida formato via Regex compilada (`^[^@\s]+@[^@\s]+\.[^@\s]+$`)
- Normaliza para letras minúsculas antes de armazenar
- Armazena em `Address`

### [`ValueObjects/Phone.cs`](./ValueObjects/Phone.cs)
Value Object para **telefone brasileiro**:
- Remove caracteres não numéricos e o DDI `55` se presente
- Aceita 10 dígitos (telefone fixo: DDD + 8 dígitos) ou 11 dígitos (celular: DDD + 9 dígitos)
- Armazena apenas os dígitos em `Number`

## Regras desta Camada

- ❌ Sem referência a Entity Framework, HttpClient ou qualquer infraestrutura
- ❌ Sem injeção de dependência (sem construtores com serviços)
- ✅ Todos os construtores de entidades e VOs são **privados** — uso obrigatório de Factory Methods
- ✅ Retornos de Factory Methods sempre usam `Result<T>` — nunca lançam exceções para erros de negócio
