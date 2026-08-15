# Mapeamento de Funcionalidades: VetNexus (SaaS)

Este documento detalha todas as funções e capacidades do sistema VetNexus, operando em modelo offline-first, multiplataforma e estritamente modular.

---

## Módulo Base & CRM (Gestão de Clientes e Pets)
* Cadastro completo de clientes (tutores) e animais.
* Centralização do histórico de atendimentos e serviços prestados.
* Perfis de acesso personalizados para usuários e controle de permissões.
* Configuração de teclas de atalho, filtros e organização de listagens.

---

## Módulo de Atendimento Clínico e Internação
* Agenda unificada para médicos veterinários e marcação de consultas.
* Prontuário veterinário completo com suporte para anexar fotos, vídeos e exames.
* Criação de exames clínicos, documentação padronizada e modelos de receita médica.
* Registro de protocolos de vacina com alertas gerados para vacinas previstas ou atrasadas.
* Elaboração de orçamentos clínicos e integração direta com o caixa.
* Registro de animais internados, histórico de tratamentos e procedimentos.
* Acompanhamento de medicamentos, prescrições de horário, evolução do paciente e mapa de execução.

---

## Módulo de Estética (Banho & Tosa Digital)
* Agenda de serviços específica para banhistas e tosadores.
* Ficha digital de banho e tosa vinculada ao histórico do pet.
* Controle de pacotes pré-pagos de banho e tosa com histórico de uso.
* Registro e abatimento automático do consumo de insumos (como shampoo) do estoque.
* Envio de notificações automáticas e em tempo real para os clientes sobre o início e término dos serviços.

---

## Módulo de Vendas e PDV Offline
* Ponto de venda (PDV) integrado operando com funcionamento 100% offline.
* Integração direta com maquininhas de cartão via TEF/APIs.
* Controle e motor de cálculo de comissões para vendedores, tosadores e veterinários.
* Gerenciamento de pacotes, kits de produtos e aplicação de limite de desconto.
* Registro e controle de devoluções de venda.

---

## Módulo de Estoque Inteligente
* Análise automática de estoque com sugestão e pedidos de compras ao fornecedor.
* Alerta de produtos com baixo estoque e controle rigoroso de validade.
* Entrada ágil de notas de compra via importação de arquivo XML.
* Controle de perdas por validade, consumo interno, avarias, doações e fracionamento.
* Realização de inventário utilizando dispositivos móveis para leitura de código de barras.
* Geração e impressão de etiquetas para produtos.
* Registro de devolução de compras ao fornecedor e saídas de estoque.

---

## Módulo Financeiro
* Gestão completa de contas a pagar e contas a receber.
* Controle de clientes em débito/crédito e projeção de saldos previstos contra reais.
* Movimentações financeiras de abertura, encerramento de caixa e controle de sangrias.
* Conciliação de cartões de crédito e débito.
* Geração de fluxo de caixa e demonstrativo financeiro mensal.

---

## Módulo Fiscal e Tributário
* Emissão de notas fiscais eletrônicas de produto e serviço (NF-e modelo 55, NFC-e e NFS-e).
* Emissão de NFC-e com contingência offline e transmissão automática ao recuperar a rede.
* Integração com sistemas fiscais municipais e estaduais.
* Planejamento fiscal com relatórios detalhados, auxílio no enquadramento tributário e estratégias para reduzir impostos.

---

## Módulo de Automação, Marketing e Relacionamento
* Envio de mensagens e lembretes automáticos via WhatsApp, SMS e e-mail.
* Gatilhos para campanhas de marketing baseadas em agendamentos, retornos, vacinas e aniversários.
* Ferramenta de disparo e análise de pesquisa de satisfação (NPS) com clientes.
* Controle da frequência de retorno de clientes ao estabelecimento.

---

## Portal do Tutor e E-commerce
* Aplicativo e portal exclusivo para o cliente (tutor) acompanhar a saúde do pet.
* Visualização da carteira de vacinas digital e histórico de exames.
* Funcionalidade para o tutor realizar o agendamento de serviços direto pelo aplicativo.
* Criação de site grátis para o estabelecimento.
* Sincronização do estoque físico gerido no backoffice com loja virtual e marketplaces.
* Gerenciamento de pedidos do e-commerce com atualização automática de preços.

---

## Módulo de Inteligência (Dashboards e BI)
* Painel visual avançado com indicadores em tempo real de vendas e serviços.
* Relatórios estratégicos de produtividade e métricas de desempenho da equipe.
* Geração de ranking de clientes (Curva ABC) e identificação de produtos mais consumidos/rentáveis.