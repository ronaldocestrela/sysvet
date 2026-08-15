# `tests/Modules/Core.Tests/` — Testes do Módulo Core

Projeto de **testes unitários** para o módulo `Core`. Cobre as entidades de domínio, Value Objects e o padrão `Result<T>`. É o projeto de testes mais maduro do repositório.

## Estrutura de Arquivos

```
Core.Tests/
├── Domain/
│   ├── Entities/
│   │   ├── TutorTests.cs          ← Testes de TutorTests
│   │   └── PetTests.cs            ← Testes de Pet
│   └── ValueObjects/
│       ├── CpfTests.cs            ← Testes de validação de CPF
│       ├── EmailTests.cs          ← Testes de validação de e-mail
│       └── PhoneTests.cs          ← Testes de validação de telefone
└── ResultTests.cs                 ← Testes do padrão Result<T>
```

## O que cada arquivo testa

### [`ResultTests.cs`](./ResultTests.cs)
Testa o padrão `Result<T>` definido em `Core.Domain.Result`:
- `Success()` deve retornar `IsSuccess=true` e `Error=None`
- `Failure(error)` deve retornar `IsFailure=true` e o erro correto
- `Success<TValue>(value)` deve retornar o valor corretamente
- Acessar `.Value` de um `Result` de falha deve lançar `InvalidOperationException`

### [`Domain/Entities/TutorTests.cs`](./Domain/Entities/TutorTests.cs)
Testa a entidade `Tutor` e seu Factory Method `Create()`:
- Criação com dados válidos retorna sucesso
- Nome com menos de 2 caracteres retorna falha com código `Tutor.InvalidName`
- E-mail, CPF ou telefone nulos retornam falha com erros específicos
- `AddPet()` com pet nulo retorna falha

### [`Domain/Entities/PetTests.cs`](./Domain/Entities/PetTests.cs)
Testa a entidade `Pet` e seu Factory Method `Create()`:
- Criação com dados válidos retorna sucesso
- Nome vazio retorna falha com código `Pet.InvalidName`
- `TutorId` igual a `Guid.Empty` retorna falha com `Pet.InvalidTutor`

### [`Domain/ValueObjects/CpfTests.cs`](./Domain/ValueObjects/CpfTests.cs)
Testa o Value Object `Cpf`:
- CPF válido (com e sem máscara) retorna sucesso
- CPF vazio retorna falha
- CPF com dígitos todos iguais retorna falha
- CPF com dígitos verificadores incorretos retorna falha
- `ToString()` formata corretamente no padrão `000.000.000-00`

### [`Domain/ValueObjects/EmailTests.cs`](./Domain/ValueObjects/EmailTests.cs)
Testa o Value Object `Email`:
- E-mail válido retorna sucesso
- E-mail é normalizado para letras minúsculas
- E-mail sem `@` retorna falha
- E-mail vazio retorna falha

### [`Domain/ValueObjects/PhoneTests.cs`](./Domain/ValueObjects/PhoneTests.cs)
Testa o Value Object `Phone`:
- Celular com 11 dígitos (com DDD) retorna sucesso
- Telefone fixo com 10 dígitos retorna sucesso
- Remove o DDI `55` automaticamente
- Telefone com menos de 10 dígitos retorna falha

## Framework

- **xUnit** como runner de testes
- Padrão **AAA** (Arrange / Act / Assert) com comentários explícitos em cada teste
