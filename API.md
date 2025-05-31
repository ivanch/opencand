# OpenCand API

## specs
### GET /v1/stats
Returns statistics about the OpenCand platform.
```json
{
  "totalCandidatos": 1000,
  "totalBemCandidatos": 2500,
  "totalEleicoes": 500
}
```

### GET /v1/candidato/search?q={query}
Search for candidates by name or other attributes.
```json
{
  "candidatos": [
    {
        "id": "6c2be869-339c-47d0-aeb6-77c686e528b5",
        "nome": "João Silva",
        "cpf": "123.***.789-10",
        "dataNascimento": "1990-01-01",
        "email": "email@test.com",
        "estadoCivil": "Solteiro",
        "sexo": "Masculino",
        "ocupacao": "1234-5678",
    }
  ]
}
```

### GET /v1/candidato/{id}
Get detailed information about a specific candidate by ID.
```json
{
    "id": "6c2be869-339c-47d0-aeb6-77c686e528b5",
    "nome": "João Silva",
    "cpf": "123.***.789-10",
    "dataNascimento": "1990-01-01",
    "email": "email@test.com",
    "estadoCivil": "Solteiro",
    "sexo": "Masculino",
    "ocupacao": "1234-5678",
    "eleicoes": [
        {
            "sqid": "160002325330",
            "tipoeleicao": "ESTADUAL",
            "siglaUf": "SP",
            "nomeue": "São Paulo",
            "nrCandidato": "123456",
            "nomeCandidato": "João Silva",
            "resultado": "ELEITO"
        }
    ]
}
```

### GET /v1/candidato/{id}/bens
Get the assets of a specific candidate by ID.
```json
{
    "bens": [
        {
            "idCandidato": "6c2be869-339c-47d0-aeb6-77c686e528b5",
            "ano": 2020,
            "tipoBem": "Apartamento",
            "descricao": "Apartamento",
            "valor": 250000.00,
        },
        {
            "idCandidato": "6c2be869-339c-47d0-aeb6-77c686e528b5",
            "ano": 2020,
            "tipoBem": "Veículo automotor terrestre: caminhão, automóvel, moto, etc.",
            "descricao": "Veículo VolksWagem POLO mca",
            "valor": 40000.00,
        }
    ]
}
```