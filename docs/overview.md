\# Descripción general



\## Alcance implementado



EvidenceChain implementa los dos recorridos principales solicitados:



1\. Consulta de evidencias mediante una bandeja paginada en servidor, con búsqueda por texto, filtro por custodio y estado de integridad, orden por fecha y persistencia de filtros en la URL.

2\. Transferencia de custodia entre usuarios, incluyendo solicitud, aceptación, rechazo, idempotencia, control de concurrencia optimista y manejo de conflictos.



La vista de detalle presenta la información de la evidencia, su línea de tiempo de custodia, anomalías y la verificación criptográfica de la cadena.



Los eventos son append-only y se enlazan mediante SHA-256. El endpoint de verificación identifica el primer evento cuya secuencia, `PreviousHash` o contenido no coincide con el hash almacenado.



El seed genera 1.000 evidencias y 10.000 eventos. Incluye una cadena íntegra, una cadena alterada y una transferencia pendiente vencida.



También se implementaron:



\- Autenticación JWT local.

\- Roles Investigador, Custodio y Supervisor.

\- Respuestas de error `application/problem+json`.

\- Protección contra respuestas de búsqueda obsoletas.

\- Actualización optimista y rollback ante errores o conflictos `409`.

\- Swagger en desarrollo.

\- Pruebas automatizadas de backend y frontend.



\## Elementos fuera del alcance



No se implementaron carga de archivos, almacenamiento real en Azure Blob Storage, rate limiting, internacionalización ni despliegue mediante infraestructura como código. Estos elementos eran opcionales y no sustituyen los recorridos obligatorios.



La arquitectura Azure propuesta se documenta en `decisions.md`.



\## Ejecución local



\### Requisitos



\- .NET SDK 8

\- Node.js 22 o compatible

\- Docker Desktop

\- SQL Server ejecutado mediante Docker



\### Base de datos



Desde la raíz:



```powershell

docker compose up -d

