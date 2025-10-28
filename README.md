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
