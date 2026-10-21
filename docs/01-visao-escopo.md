# 01 — Visão e escopo

## Contexto informado

Distribuição B2B de fermento, misturas e outros insumos perecíveis para panificadoras, MEIs e empresas de alimentação. A operação envolve praças exclusivas, vendedores externos, processamento central de pedidos e crédito de curto prazo.

## Resultados esperados

- Registrar pedidos mesmo sem conexão e transmiti-los sem duplicação.
- Evitar concorrência comercial indevida dentro da mesma praça.
- Permitir corte central auditável quando a demanda exceder o estoque.
- Reduzir perdas por validade com visibilidade de lotes e promoções.
- Reconciliar expedição, entrega, documentos fiscais e cobrança.

Os indicadores serão medidos no piloto: pedidos sincronizados/pendentes, tempo de processamento, quantidade cortada por produto, perdas por validade e divergências de entrega. Metas numéricas dependem da medição inicial.

## Perfis

| Perfil | Responsabilidade |
| --- | --- |
| Administrador | Usuários, configurações e permissões |
| Supervisor comercial | Carteiras, praças, preços, promoções e cortes |
| Vendedor | Roteiro, visitas, pedidos e consulta financeira da carteira |
| Estoquista | Lotes, movimentos, separação e conferência |
| Financeiro | Condições, títulos, recebimentos e ajustes |
| Logística | Cargas, entregas e ocorrências |

Apenas usuários autorizados podem acumular funções. Consulta financeira não implica permissão para baixar títulos.

## MVP proposto

Uma distribuidora, uma praça piloto e um depósito operacional inicial, sujeitos à confirmação D01. Cadastro de clientes/produtos; carteira e roteiro; aplicativo offline; sincronização; estoque por lote; corte manual assistido; reserva; separação; expedição; registro de entrega; parcelas e recebimentos manuais; auditoria.

O piloto usará emissor fiscal e cobrança existentes, com referências externas registradas no sistema. Expedição comercial depende da conferência do documento fiscal externo. Isso não equivale a uma integração automática nem dispensa o processo fiscal aplicável.

## Evolução posterior

Integrações automáticas NF-e, boletos e Pix; aplicativo do motorista; otimização de rotas; múltiplos depósitos em operação; regras avançadas de crédito; corte automático por prioridade; portal do cliente e comissões. Não existe decisão de utilizar Sicoob ou outro banco neste projeto.

## Premissas propostas

- Quantidades em unidades comerciais inteiras, respeitando múltiplos de embalagem.
- Uma moeda operacional: BRL.
- Fuso comercial inicial: America/Sao_Paulo; instantes persistidos em UTC.
- Vendedor só consulta dados da própria carteira.
- Rascunho é editável; pedido recebido só muda por ação controlada e auditada.

Todas as premissas precisam ser ratificadas antes de afetar a implementação correspondente.
