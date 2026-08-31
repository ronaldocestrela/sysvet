# Configuração e Observabilidade (SysVet)

Este documento descreve as decisões arquiteturais relacionadas à configuração da aplicação, observabilidade e verificações de integridade.

## 1. Options Tipados e Fail-Fast
Seguindo as melhores práticas do .NET, as configurações não são acessadas diretamente do `IConfiguration`. Utilizamos o **Options Pattern** tipado e implementamos a estratégia de **Fail-Fast** usando `ValidateDataAnnotations()` e `ValidateOnStart()`.

### Como Funciona?
Isso significa que todas as propriedades vitais (como `JwtSettings.Secret`, `TenancySettings.DefaultSchema`) têm anotações (`[Required]`, `[MinLength]`, etc.). Se a aplicação inicializar com um `appsettings.json` inválido, ela irá "falhar rápido" e travar no boot, impedindo o deploy de um software quebrado em produção.

## 2. Rastreamento (Trace e Correlation ID)
Foi introduzido o `CorrelationIdMiddleware` para criar um rastro unificado (Trace) das requisições. 
- Quando uma requisição entra, buscamos o header `X-Correlation-Id`.
- Caso não exista, um ID único de `Activity.Current` ou `TraceIdentifier` é injetado.
- Este ID é retornado no header de resposta e anexado ao log da aplicação através do `ILogger.BeginScope`. Isso nos permite filtrar nos agregadores de log (como Datadog ou Kibana) todos os logs associados a uma única requisição.

## 3. Health Checks
Os Health Checks foram separados em duas categorias seguindo os padrões do Kubernetes:
- **/health/live (Liveness Probe)**: Retorna apenas se a aplicação web subiu e está respondendo tráfego (ignorando os bancos de dados). Ajuda o orquestrador a saber se a API "morreu" (necessitando de restart).
- **/health/ready (Readiness Probe)**: Avalia a saúde de todas as dependências injetadas (Banco de dados, Serviços Externos, Message Brokers, etc.). Ajuda o Load Balancer a decidir se a API está pronta para *receber* requisições de clientes sem gerar 500.
