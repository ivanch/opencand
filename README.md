# OpenCand

O [OpenCand](https://opencand.ivanch.me) é uma plataforma para explorar de forma intuitiva e fácil os dados eleitorais brasileiros, tendo o objetivo de ser uma alternativa para o acesso às informações públicas do Tribunal Superior Eleitoral (TSE).

Atualmente é um projeto desenvolvido como hobby, onde apenas o código do projeto de ETL (Extract-Transform-Load) e API estão disponíveis, juntamente com o banco de dados final para download (formato `.sql`).

## Arquitetura

O OpenCand é construído usando:
* **.NET Core 8**  
  Para analisar informações iniciais dos arquivos CSV provenientes do TSE para o banco de dados PostgreSQL.  
  Também é usado para a API RESTful que serve os dados para o front-end, possuindo um cache interno.
* **PostgreSQL**  
  Para o banco de dados.
* **React**  
  Para o front-end, junto com TypeScript, Tailwind CSS e Vite.

Para o deploy, o projeto utiliza Docker e Docker Compose, juntamente com Gitea Actions para CI/CD. Há também monitoramento com Prometheus, Loki e Grafana, porém não estão disponíveis publicamente.

## Estrutura do projeto

```
.
├── OpenCand.API      # Executa a API que serve dados para o front-end
├── OpenCand.Core     # Modelos e serviços compartilhados entre API e ETL
├── OpenCand.ETL      # Responsável por importar os dados dos CSVs para o PostgreSQL
└── db                # Contém a estrutura para o banco de dados
```

## Executando o ETL

O ETL lê os CSVs do TSE (pasta apontada por `BasePath`, subpastas em `CsvSettings`) e grava no PostgreSQL:

```bash
docker compose --profile etl run --rm etl                       # tudo
docker compose --profile etl run --rm etl --only candidatos,bens # só alguns conjuntos
```

* Conjuntos: `candidatos`, `bens`, `redes`, `despesas`, `receitas` (nessa ordem; os demais dependem de `candidatos`).
* Os CSVs são lidos em streaming e gravados com `COPY` binário. A pasta de entrada pode ser somente leitura.
* `candidatos`, `bens` e `redes` são *upserts* incrementais. `despesas` e `receitas` são reconstruídas em uma tabela `*_new` e só substituem a tabela atual (junto com as views materializadas, na mesma transação) depois que todos os arquivos foram carregados. Se algo falhar, as tabelas atuais permanecem intactas.
* O processo termina com código `0` (sucesso), `1` (falha) ou `2` (argumentos inválidos). Cada execução registra uma linha em `etl_run`.
* Configuração (`Etl` em `appsettings.json` ou `Etl__*` como variável de ambiente): `BatchSize`, `MaxParallelFiles`, `MaxRejectRatio` (padrão 1%: acima disso o conjunto falha), `CsvEncoding`, `RebuildViews`.
* Antes da primeira execução em um banco existente, aplique `db/migrations/001_etl_and_fixes.sql` (faça um `pg_dump` antes) e depois `db/mv.sql`.

