# 07 — Backlog e entregas

Estimativas serão produzidas após fechar escopo, integrações, disponibilidade da equipe e volumes. A ordem abaixo é de dependência, não promessa de calendário.

| Entrega | Conteúdo | Requisitos | Dependência | Aceite |
| --- | --- | --- | --- | --- |
| E0 — Refinamento | Fechar D01–D05, protótipos e exemplos; detalhar D06–D13 | Todos | Nenhuma | Decisões registradas com responsável e data |
| E1 — Fundação | Solução, versões, banco, migrations, CI e identidade | RF01, RF16 | E0 | API saudável; permissões verificadas; build reproduzível |
| E2 — Comercial | Clientes, territórios, carteiras, roteiro, catálogo e preços | RF02–RF04 | E1 | Carteira piloto e catálogo carregados com dados fictícios |
| E3 — Coleta | SQLite, rascunhos, visitas, fila e sincronização | RF05–RF07 | E2 | Modo avião, reinício e reenvio aprovados |
| E4 — Estoque e corte | Entradas, lotes, validade, demanda, simulação e reserva | RF08–RF10 | E2/E3 | Concorrência não ultrapassa saldo; corte auditável |
| E5 — Expedição | Separação, venda, referência fiscal externa, carga e entrega | RF11–RF13 | E4 | Entrega parcial rastreada sem apagar faturamento |
| E6 — Financeiro | Parcelas, formas, pendências, recebimentos e ajustes | RF14–RF15 | E5 | Saldos e estornos conciliados |
| E7 — Piloto | Relatórios, restauração, segurança, capacitação e operação assistida | RF16–RF17, RNFs | E1–E6 | Roteiro ponta a ponta aprovado |
| E8 — Integrações | Fiscal, bancos e motorista conforme prioridade | RF18 | E7 e fornecedores definidos | Homologação e reconciliação de falhas |

## Histórias prioritárias

- **US01 / E2:** como supervisor, vincular praça ao vendedor para impedir disputa interna. Aceite: atribuições sobrepostas são recusadas.
- **US02 / E3:** como vendedor, coletar sem conexão para não perder a visita. Aceite: pedido sobrevive ao reinício e aparece como pendente.
- **US03 / E3:** como vendedor, reenviar após timeout sem duplicar. Aceite: mesmo UUID retorna o pedido já recebido.
- **US04 / E4:** como supervisor, distribuir saldo insuficiente. Aceite: exemplo 50/30/20 com saldo 70 resulta em 35/21/14 antes de ajustes.
- **US05 / E5:** como estoquista, selecionar lote elegível. Aceite: lote vencido é impedido mesmo que tenha sido reservado anteriormente.
- **US06 / E5:** como logística, registrar recusa parcial. Aceite: conserva quantidades e gera pendência de conciliação.
- **US07 / E6:** como financeiro, parcelar e baixar parcialmente. Aceite: soma das parcelas fecha o total e saldo reflete recebimento/estorno.
- **US08 / E7:** como administrador, restaurar a operação após falha. Aceite: base restaurada em ambiente isolado e saldos conferidos.

## Definição de pronto

Critérios de aceite atendidos; revisão de código; testes relevantes de domínio/integração; autorização no servidor; migrations revisadas; nenhum segredo no Git; logs úteis sem dados excessivos; documentação atualizada; demonstração registrada na issue.

## Primeira tarefa de implementação

Após E0, criar a solução e projetos da arquitetura, fixar SDK/dependências compatíveis, configurar PostgreSQL de desenvolvimento, gerar primeira migration e validar autenticação/escopo. Não implementar regras fiscais ou percentuais financeiros ainda pendentes.
