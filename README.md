# Panis360

Sistema de Distribuição de Panificação B2B

Documentação inicial — versão 0.1 — 21/10/2026.

**Situação:** planejamento para revisão. Este repositório ainda não contém uma aplicação executável. Nome do projeto: **Panis360**. O namespace técnico `Panificacao` usado nos exemplos será confirmado na criação da solução.

## Objetivo

Controlar visitas comerciais, pedidos offline, distribuição de estoque perecível, separação, entrega e recebimentos de uma distribuidora de insumos para panificação.

O pedido coletado não confirma estoque nem constitui, sozinho, uma venda faturada. O sistema preservará as quantidades solicitada, aprovada, separada, faturada e entregue.

## Documentação

1. [Visão e escopo](docs/01-visao-escopo.md)
2. [Requisitos e critérios de aceite](docs/02-requisitos.md)
3. [Regras e fluxos operacionais](docs/03-regras-fluxos.md)
4. [Arquitetura e sincronização](docs/04-arquitetura.md)
5. [Modelo conceitual de dados](docs/05-modelo-dados.md)
6. [Contratos iniciais da API](docs/06-api.md)
7. [Backlog e entregas](docs/07-backlog.md)
8. [Qualidade, segurança e implantação](docs/08-qualidade-operacao.md)
9. [Decisões pendentes](docs/09-decisoes-pendentes.md)
10. [Guia para salvar no GitHub](docs/10-guia-github.md)
11. [ADR 001: arquitetura proposta](docs/adr/001-arquitetura.md)

## Tecnologias propostas

Aplicativo Android em .NET MAUI com SQLite; administrativo em Blazor; API ASP.NET Core; Entity Framework Core; PostgreSQL. As versões e dependências serão fixadas na criação do código, após verificar compatibilidade e suporte.

## Como começar

1. Revisar o escopo e responder às decisões D01 a D05.
2. Validar os exemplos de corte, entrega parcial e cobrança.
3. Versionar as atualizações no repositório `vanderleifonseca/Panis360`.
4. Executar a entrega E0 do backlog e iniciar a fundação E1.

## Origem e limites

Baseado nas especificações textuais fornecidas por Vanderlei nesta conversa e no planejamento derivado delas. O conteúdo do link do NotebookLM não foi acessado nem validado. Regras informadas, propostas de implementação e pendências são distinguidas nos documentos. Este material não substitui a validação dos processos fiscais, bancários e contratuais da empresa.

## Histórico

| Versão | Data | Alteração |
| --- | --- | --- |
| 0.1 | 21/10/2026 | Base inicial de requisitos, arquitetura, dados, API e backlog |
