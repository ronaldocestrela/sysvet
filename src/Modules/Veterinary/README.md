# `src/Modules/Veterinary/` — Módulo Clínico Veterinário

Módulo responsável por todas as operações **clínicas** da clínica veterinária: prontuários, consultas, internações, vacinas, prescrições e histórico médico dos animais.

## Status

> 🔴 **Não iniciado.** As subpastas de camada existem mas estão vazias (apenas `.gitkeep` e arquivos de projeto).

## Escopo de Negócio

Este módulo gerenciará:
- **Prontuários**: histórico médico completo por animal
- **Consultas**: agendamento, registro de anamnese, diagnóstico e tratamento
- **Internações**: controle de animais hospitalizados com evolução diária
- **Vacinação**: carteira de vacinação, alertas de reforço e vencimentos
- **Prescrições**: medicamentos prescritos, dosagem e duração do tratamento
- **Exames**: solicitação e resultado de exames laboratoriais e de imagem

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades: `MedicalRecord`, `Consultation`, `Hospitalization`, `Vaccine`, `Prescription`. Value Objects: `Diagnosis`, `Dosage`. |
| [`Application/`](./Application/) | Commands: `CreateConsultation`, `RegisterVaccine`, `AdmitAnimal`. Queries: `GetMedicalHistory`, `GetActiveHospitalizations`. |
| [`Infrastructure/`](./Infrastructure/) | `VeterinaryDbContext`, repositórios EF Core, integração com APIs de exames (futuro). |

## Dependências

- Referencia `Core.Domain` para consumir as entidades `Pet` e `Tutor` (somente leitura via Id)
- **Não** referencia nenhum outro módulo de negócio
