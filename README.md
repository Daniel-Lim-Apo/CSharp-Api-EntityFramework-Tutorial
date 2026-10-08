# LojaApi — ASP.NET Core + Entity Framework Core

Projeto de apoio para a aula de Persistência de Dados, ORM e Validação.

## 1. Criar o projeto do zero

```bash
dotnet new webapi --use-controllers -n LojaApi
cd LojaApi
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet tool install --global dotnet-ef
```

Se `dotnet-ef` já estiver instalado:

```bash
dotnet tool update --global dotnet-ef
```

## 2. Criar o banco por migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 3. Executar

```bash
dotnet run --urls http://localhost:5050
```

## 4. Testar

### Swagger UI (Documentação e Testes Interativos)

Acesse no navegador:
- [http://localhost:5050/swagger](http://localhost:5050/swagger)

### REST Client (Extensão do VS Code)

Abra os arquivos da pasta `HttpTests/` e clique em **"Send Request"** acima de cada requisição:
- `HttpTests/products.http`: Testes completos de CRUD e filtros.
- `HttpTests/stock.http`: Testes de movimentação de estoque (entradas, saídas e regras de negócio).
- `HttpTests/validations.http`: Testes de validação de DTOs (Data Annotations) e erros 400/404.

### cURL

```bash
curl.exe http://localhost:5050/api/products

curl.exe -X POST http://localhost:5050/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Mechanical Keyboard","price":399.90,"stock":10,"categoryId":1}'

curl.exe "http://localhost:5050/api/products?minPrice=100&categoryId=1"

curl.exe -X POST http://localhost:5050/api/products/1/stock \
  -H "Content-Type: application/json" \
  -d '{"quantity":-2}'
```

No PowerShell, use crase (`) para quebrar linhas, ou execute cada comando em uma única linha.

## Pontos de aprendizagem

- Entidades, relacionamentos 1:N e chaves estrangeiras.
- `DbContext`, `DbSet<T>` e injeção de dependência.
- Fluent API, índices e seed data.
- Migrations e evolução do schema.
- CRUD assíncrono com LINQ.
- `AsNoTracking`, projeção com `Select` e filtros no banco.
- Validação de DTOs com Data Annotations e `[ApiController]`.
- `ProblemDetails` e tratamento global de exceções.
- Transações explícitas quando há mais de um `SaveChangesAsync` que precisa ser atômico.
