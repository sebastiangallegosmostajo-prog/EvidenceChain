\# EvidenceChain



Aplicación full stack para consultar evidencia digital, verificar la integridad de su cadena de custodia y transferirla entre responsables.



La solución fue desarrollada con:



\- .NET 8 y ASP.NET Core

\- Entity Framework Core

\- SQL Server 2022

\- React 19

\- TypeScript

\- Vite

\- Vitest

\- Docker Compose



\## Funcionalidades



\- Autenticación JWT local.

\- Roles Investigador, Custodio y Supervisor.

\- Bandeja de evidencias paginada en servidor.

\- Búsqueda por código o descripción.

\- Filtros por custodio y estado de integridad.

\- Filtros conservados en la URL.

\- Detalle y línea de tiempo de custodia.

\- Eventos append-only encadenados mediante SHA-256.

\- Verificación del primer evento inválido.

\- Detección de transferencias pendientes vencidas.

\- Solicitud, aceptación y rechazo de transferencias.

\- Idempotencia mediante `Idempotency-Key`.

\- Concurrencia optimista mediante `rowversion` e `If-Match`.

\- Respuestas de error `application/problem+json`.

\- Actualización optimista y rollback ante conflictos.

\- Swagger/OpenAPI.

\- Seed reproducible de 1.000 evidencias y 10.000 eventos.



\## Estructura



```text

EvidenceChain/

├── backend/

│   ├── src/

│   │   ├── EvidenceChain.Api/

│   │   ├── EvidenceChain.Application/

│   │   ├── EvidenceChain.Domain/

│   │   └── EvidenceChain.Infrastructure/

│   └── tests/

├── frontend/

├── docs/

│   ├── overview.md

│   ├── decisions.md

│   ├── code-map.md

│   ├── ai-usage.md

│   └── ai-code-review.md

├── docker-compose.yml

├── openapi.yaml

└── README.md

