# ADR 001: Arquitetura Monolito Modular

## Status
Aceito

## Contexto
O SysVet precisa evoluir para suportar multi-tenancy e uma eventual escalabilidade, mantendo a simplicidade de implantação nas fases iniciais e suporte a operação offline em clínicas veterinárias. Adotar microsserviços desde o início aumentaria significativamente a complexidade operacional.

## Decisão
Adotaremos a arquitetura de **Monolito Modular** utilizando .NET 10. O sistema será dividido em módulos lógicos (Core, Veterinary, Petshop, Sales, Inventory, Fiscal), onde cada módulo terá forte isolamento de domínio e infraestrutura, mas compartilhará o mesmo processo em tempo de execução e, inicialmente, o mesmo banco de dados (através de schemas isolados).

## Consequências
- **Positivas**: Facilidade de implantação e testes (um único processo); Isolamento de código facilita futura extração para microsserviços se necessário.
- **Negativas**: Requer disciplina rigorosa da equipe para não criar acoplamento direto entre os módulos (uso obrigatório de interfaces e mensageria).
