# ADR 001 — Monólito modular com aplicativo offline

- Data: 21/10/2026.
- Status: proposto.
- Decisores: responsáveis técnicos e de negócio, a designar.

## Contexto

Vendedores coletam pedidos em campo, sem garantir conexão ou estoque. A central precisa executar cortes e reservas consistentes. A equipe pretende trabalhar com C# e manter a operação compreensível no início.

## Decisão proposta

Monólito modular com ASP.NET Core e PostgreSQL; Blazor para administrativo online; .NET MAUI Android e SQLite para coleta offline. API centraliza autorização e regras de negócio. Processamento assíncrono usa outbox quando necessário.

## Alternativas

| Alternativa | Avaliação |
| --- | --- |
| Microsserviços desde o início | Maior custo de operação e consistência distribuída sem necessidade demonstrada |
| Aplicativo apenas online | Simplifica sincronização, mas não atende coleta em conectividade limitada |
| Web/PWA offline | Possível alternativa; validar requisitos de dispositivo e persistência antes de substituir MAUI |

## Consequências

Menos componentes para operar e transações locais para estoque. Exige disciplina de módulos e protocolo de sincronização. Tecnologia mobile e administrativo devem ser reavaliados se surgirem requisitos de iOS, navegador offline ou equipe diferente.

## Revisão

Ratificar em E0/E1. Definir versões compatíveis, autenticação, implantação e proteção local antes do piloto. Mudanças relevantes criam novo ADR e referenciam este documento.
