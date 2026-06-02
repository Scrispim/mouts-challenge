# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories.

See [Project Structure](/.doc/project-structure.md)

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) and Docker Compose

### Configuration

1. Clone the repository
2. Copy the environment file and adjust if needed:
   ```bash
   cp .env.example .env   # or edit .env directly
   ```
   Default values in `.env`:
   ```
   POSTGRES_DB=motus_db
   POSTGRES_USER=dev_user
   POSTGRES_PASSWORD=dev123
   ```

### Running with Docker (recommended)

```bash
docker compose up --build
```

The API will be available at `http://localhost:8080`.
Swagger UI: `http://localhost:8080/swagger`

### Running locally

1. Start the database:
   ```bash
   docker compose up db
   ```

2. Run the API:
   ```bash
   dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
   ```

The API will be available at `https://localhost:5001` / `http://localhost:5000`.

### Running Tests

**Unit tests:**
```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit
```

**Integration tests:**
```bash
dotnet test tests/Ambev.DeveloperEvaluation.Integration
```

**All tests:**
```bash
dotnet test
```

### API Overview

Base URL: `/api/sales`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/sales` | List sales (paginated, filterable, orderable) |
| `GET` | `/api/sales/{id}` | Get sale by ID |
| `POST` | `/api/sales` | Create a new sale |
| `PUT` | `/api/sales/{id}` | Update a sale |
| `DELETE` | `/api/sales/{id}` | Cancel a sale |
| `DELETE` | `/api/sales/{id}/items/{itemId}` | Cancel a sale item |

**Query parameters for GET /api/sales:**

| Parameter | Description | Example |
|-----------|-------------|---------|
| `_page` | Page number (default: 1) | `?_page=2` |
| `_size` | Page size (default: 10) | `?_size=20` |
| `_order` | Sort fields | `?_order=saleDate desc` |
| `customerName` | Filter by customer name (supports `*` wildcard) | `?customerName=John*` |
| `branchName` | Filter by branch name | `?branchName=*Store` |
| `saleNumber` | Filter by sale number | `?saleNumber=SALE-001` |
| `isCancelled` | Filter by cancelled status | `?isCancelled=false` |
| `_minSaleDate` | Filter sales from date | `?_minSaleDate=2024-01-01` |
| `_maxSaleDate` | Filter sales up to date | `?_maxSaleDate=2024-12-31` |
