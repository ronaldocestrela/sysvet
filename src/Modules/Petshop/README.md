# `src/Modules/Petshop/` — Módulo de Estética e Petshop

Módulo responsável pelos serviços de **estética animal**: banho, tosa, hidratação e outros serviços de beleza pet. Gerencia a agenda do salão e o histórico de serviços por animal.

## Status

> 🔴 **Não iniciado.** As subpastas de camada existem mas estão vazias (apenas `.gitkeep` e arquivos de projeto).

## Escopo de Negócio

Este módulo gerenciará:
- **Agendamentos**: marcação de banho e tosa com horário, funcionário e tipo de serviço
- **Pacotes de serviços**: banho simples, tosa higiênica, banho + tosa completa, hidratação
- **Histórico**: registro dos serviços realizados por animal com observações (ex: "pelagem sensível")
- **Fila de atendimento**: visualização da fila do dia em tempo real
- **Notificações**: lembretes automáticos para tutores

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades: `Appointment`, `GroomingService`. Enums: `ServiceType`, `AppointmentStatus`. Value Objects: `ServiceDuration`. |
| [`Application/`](./Application/) | Commands: `ScheduleAppointment`, `CompleteService`, `CancelAppointment`. Queries: `GetDailySchedule`, `GetPetGroomingHistory`. |
| [`Infrastructure/`](./Infrastructure/) | `PetshopDbContext`, repositórios. |

## Dependências

- Referencia `Core.Domain` para `Pet` e `Tutor` via Id
- Integra-se ao módulo `Sales` para geração de cobrança após conclusão do serviço (via evento de domínio)
