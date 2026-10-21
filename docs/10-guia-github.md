# 10 — Como salvar no GitHub

Este pacote contém documentação Markdown e modelos de issue/PR. Não contém código executável. Repositório definido: `vanderleifonseca/Panis360`. As instruções abaixo também servem para atualizações futuras; não é necessário criar outro repositório.

## Opção A — Pelo navegador, sem comandos

1. Extraia o ZIP. Abra a pasta `panificacao-b2b` e confirme a presença de `README.md` e `docs`.
2. No GitHub, crie um repositório com o nome escolhido e selecione a visibilidade desejada. Recomenda-se privado enquanto o projeto está em definição.
3. Para um repositório novo e vazio, use **uploading an existing file**. Para um existente, **Add file → Upload files**.
4. Envie o conteúdo interno da pasta: `README.md`, `docs`, `.github`, `.gitignore` e `CONTRIBUTING.md`. Não envie apenas o ZIP; o GitHub precisa dos arquivos extraídos para mostrar a documentação.
5. Revise a lista e confirme o commit com a mensagem `docs: adiciona planejamento inicial do sistema B2B`.
6. Abra o README no GitHub e teste os links de documentação.

Resultado esperado: README na raiz, pasta docs navegável e commit visível. Se já houver README no destino, preserve o existente e revise a mesclagem; não substitua arquivos de outro projeto sem verificar.

## Opção B — Git no PowerShell, para repositório novo vazio

Pré-requisitos: Git instalado, autenticação configurada e repositório vazio criado no GitHub. Abra o PowerShell na pasta extraída que contém README.md.

```powershell
git init -b main
```

Esperado: repositório Git inicializado na pasta. Se já existir `.git`, pare e consulte o estado antes de continuar.

```powershell
git add README.md docs .github .gitignore CONTRIBUTING.md
git status
```

Esperado: somente os arquivos deste pacote preparados para commit.

```powershell
git commit -m "docs: adiciona planejamento inicial do sistema B2B"
```

Esperado: novo commit. Se o Git solicitar nome/e-mail, configure a identidade desejada antes de repetir.

Copie a URL HTTPS exata do repositório criado e substitua o marcador abaixo; não execute o marcador literalmente.

```powershell
git remote add origin URL_HTTPS_DO_REPOSITORIO
git push -u origin main
```

Esperado: branch main enviada e vinculada ao remoto. Se o repositório remoto já tiver commits, use o fluxo de clone abaixo, sem forçar o envio.

## Repositório existente

Clone o repositório correto, crie uma branch `docs/planejamento-inicial`, copie os arquivos preservando o conteúdo já existente, revise `git diff`, faça commit, envie a branch e abra um pull request. Não use `push --force` para resolver diferenças.

## Conferência final

- README aparece na raiz e todos os links internos funcionam.
- Diagramas Mermaid são renderizados no GitHub.
- Nenhuma senha, token, certificado ou dado real de cliente foi incluído.
- A publicação é verificada na página do commit; arquivo local ou ZIP baixado não significa envio ao GitHub.
