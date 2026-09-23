\# Mapa de código



La solución utiliza una arquitectura por capas:



\- `Domain`: entidades, estados y reglas centrales.

\- `Application`: casos de uso y contratos de infraestructura.

\- `Infrastructure`: Entity Framework Core, SQL Server, seguridad y persistencia.

\- `Api`: endpoints HTTP, autenticación y manejo de errores.

\- `frontend`: interfaz React, estado de presentación y consumo de API.



| Capacidad | Archivo / módulo principal | Punto de entrada |

|---|---|---|

| Encadenado de hash | `backend/src/EvidenceChain.Domain/Services/CustodyEventHashCalculator.cs` y `Domain/Entities/CustodyEvent.cs` | Creación de cada `CustodyEvent` |

| Verificación de hash | `backend/src/EvidenceChain.Application/Evidences/VerifyChain/VerifyEvidenceChainHandler.cs` | `GET /api/v1/evidence/{id}/chain/verify` |

| Máquina de estados | `backend/src/EvidenceChain.Domain/Entities/CustodyTransfer.cs` | Métodos `Request`, `Accept` y `Reject` |

| Concurrencia optimista | Configuración EF Core de `CustodyTransfer.RowVersion` y handlers `Accept`/`Reject` | `POST /api/v1/custody-transfers/{id}/accept` y `/reject` |

| Idempotencia | `backend/src/EvidenceChain.Application/Common/Models/IdempotencyRecord.cs` y `CustodyTransfers/Request/RequestCustodyTransferHandler.cs` | `POST /api/v1/custody-transfers` con `Idempotency-Key` |

| Regla de anomalía | `backend/src/EvidenceChain.Application/Evidences/Chain/GetEvidenceChainHandler.cs` | `GET /api/v1/evidence/{id}/chain` |

| Consulta paginada | `backend/src/EvidenceChain.Application/Evidences/List/GetEvidenceListHandler.cs` | `GET /api/v1/evidence` |

| Detalle de evidencia | `backend/src/EvidenceChain.Application/Evidences/Detail/GetEvidenceDetailHandler.cs` | `GET /api/v1/evidence/{id}` |

| Autenticación JWT | `backend/src/EvidenceChain.Application/Authentication/Login/LoginHandler.cs` y `Api/Authentication/JwtTokenGenerator.cs` | `POST /api/v1/auth/login` |

| Manejo de errores | `backend/src/EvidenceChain.Api/ErrorHandling/ApiExceptionHandler.cs` | Middleware `UseExceptionHandler` |

| Seed reproducible | `backend/src/EvidenceChain.Infrastructure/Persistence/Seeding/DatabaseSeeder.cs` y `EvidenceDataSeeder.cs` | Inicio de la API en ambiente Development |

| Filtros de URL | `frontend/src/pages/EvidenceListPage.tsx` | Formulario de bandeja y `history.pushState` |

| Cancelación de solicitudes | `frontend/src/pages/EvidenceListPage.tsx` y `frontend/src/services/api.ts` | `AbortController` del efecto de consulta |

| Detalle y línea de tiempo | `frontend/src/pages/EvidenceDetailPage.tsx` | Selección de una evidencia |

| Solicitud optimista | `frontend/src/components/RequestTransferPanel.tsx` | Formulario para solicitar transferencia |

| Aceptación y rechazo | `frontend/src/pages/PendingTransfersPage.tsx` | Bandeja de transferencias pendientes |

| Rollback y manejo de 409 | `RequestTransferPanel.tsx`, `PendingTransfersPage.tsx` y `services/api.ts` | Respuesta `ApiError` con estado `409` |

| Sesión del usuario | `frontend/src/services/authService.ts` y `frontend/src/App.tsx` | Inicio y cierre de sesión |



\## Recorrido de consulta



1\. `EvidenceListPage` conserva los filtros en la URL.

2\. `evidenceService` construye la solicitud HTTP.

3\. `EvidenceController` valida los parámetros.

4\. `GetEvidenceListHandler` filtra, ordena y pagina en SQL Server.

5\. La respuesta actualiza la bandeja solamente si la solicitud no fue cancelada.



\## Recorrido de transferencia



1\. `RequestTransferPanel` genera una `Idempotency-Key`.

2\. `CustodyTransfersController` valida autenticación, autorización y cabeceras.

3\. `RequestCustodyTransferHandler` valida la evidencia y registra la solicitud.

4\. El destinatario recibe la transferencia pendiente.

5\. Los endpoints de aceptación o rechazo comparan `If-Match` con `RowVersion`.

6\. Una actualización concurrente devuelve `409 Conflict`.

7\. React revierte o reconcilia el estado optimista y muestra la explicación.

