\# Decisiones técnicas



\## 1. Persistencia y encadenado de hash



\### Opción elegida



Los eventos de custodia se almacenan como registros append-only. Cada evento contiene:



\- Identificador del evento y de la evidencia.

\- Número de secuencia.

\- Tipo de evento.

\- Actor.

\- Custodios de origen y destino.

\- Transferencia relacionada.

\- Fecha UTC.

\- Detalle.

\- `PreviousHash`.

\- `Hash`.



El primer evento utiliza como `PreviousHash` 64 caracteres `0`. Los eventos posteriores almacenan el hash del evento anterior.



El contenido se serializa en UTF-8 usando un orden fijo de propiedades, fechas normalizadas a UTC y representación estable de valores nulos. Sobre esta representación canónica se calcula SHA-256 y se almacena el resultado hexadecimal en minúsculas.



La verificación ordena por `SequenceNumber` y valida:



1\. Continuidad de la secuencia.

2\. Coincidencia de `PreviousHash`.

3\. Coincidencia entre el hash almacenado y el recalculado.



\### Alternativa descartada



Se descartó depender solamente de una tabla temporal, triggers o Azure SQL Ledger. Estas alternativas ofrecen capacidades adicionales de auditoría, pero ocultarían parte de la lógica central solicitada y aumentarían la dependencia de infraestructura.



\### Costo asumido



La integridad depende de que toda escritura pase por el dominio. La verificación completa tiene costo lineal respecto al número de eventos de una evidencia.



\### Señal para cambiar



Se reconsideraría la solución si las cadenas alcanzaran cientos de miles de eventos, si la verificación p95 superara dos segundos o si existiera un requisito regulatorio de inmutabilidad administrada externamente.



\## 2. Paginación



\### Opción elegida



Se utiliza paginación por desplazamiento mediante `Skip` y `Take`, con orden estable por `LastEventAtUtc` e `Id`. La API devuelve página, tamaño, total de registros y total de páginas.



Esta opción permite navegación directa entre páginas y resulta adecuada para el volumen inicial de 1.000 evidencias.



\### Alternativa descartada



Se evaluó keyset pagination. Ofrece mejor rendimiento en conjuntos grandes y evita algunos cambios de posición cuando existen escrituras concurrentes, pero dificulta navegar directamente a una página y requiere cursores adicionales.



\### Costo asumido



Las páginas profundas requieren que SQL Server recorra filas anteriores y `COUNT` agrega trabajo adicional.



\### Señal para cambiar



Se migraría a keyset pagination si el volumen creciera sobre 100.000 evidencias, las páginas profundas se volvieran frecuentes o el p95 de la consulta superara 500 ms.



\## 3. Concurrencia y actualización optimista



\### Opción elegida



`CustodyTransfer` utiliza una columna SQL Server `rowversion`. La API entrega este valor en Base64 y exige enviarlo mediante `If-Match` al aceptar o rechazar.



Entity Framework Core compara la versión durante la actualización. Si otro usuario modificó primero la transferencia, la operación produce un conflicto y la API responde `409 Conflict` usando `application/problem+json`, incluyendo información del estado actual.



Las escrituras utilizan `Idempotency-Key`. Se almacena la clave junto con la operación, el hash de la solicitud y el recurso generado. Repetir la misma solicitud devuelve el resultado previo; reutilizar la clave con otro contenido produce un conflicto.



En React, la solicitud aparece inmediatamente como pendiente. Si el servidor rechaza la operación o responde `409`, el estado optimista se elimina, se conserva la selección del usuario y se muestra una explicación.



\### Alternativa descartada



Se descartaron bloqueos pesimistas porque mantendrían transacciones y bloqueos durante más tiempo, reduciendo concurrencia y aumentando el riesgo de espera o deadlocks.



\### Costo asumido



El cliente debe conservar y enviar la versión correcta. Un conflicto requiere reconciliar o volver a consultar el estado del servidor.



\### Señal para cambiar



Se revisaría el diseño si la tasa de conflictos superara 1 %, si aparecieran operaciones de larga duración o si varias transferencias necesitaran confirmarse atómicamente.



\## 4. Arquitectura Azure para producción



\### Opción elegida



La SPA se desplegaría en Azure Static Web Apps. La API .NET se ejecutaría en Azure App Service para Linux. Azure SQL Database almacenaría la información transaccional.



Los archivos de evidencia, si se implementara su carga, se guardarían en Azure Blob Storage mediante contenedores privados, cifrado administrado y acceso temporal con identidades administradas o SAS de corta duración.



Azure Key Vault almacenaría secretos, cadenas de conexión y claves. App Service utilizaría una identidad administrada para acceder a Key Vault y Blob Storage, evitando secretos incluidos en configuración o repositorios.



Application Insights y Log Analytics recibirían trazas, solicitudes, excepciones, dependencias y métricas de la API.



```mermaid

flowchart TB

&#x20;   SPA\["Azure Static Web Apps"] --> API\["App Service .NET API"]

&#x20;   API --> SQL\["Azure SQL Database"]

&#x20;   API --> BLOB\["Blob Storage privado"]

&#x20;   API --> KV\["Key Vault"]

&#x20;   API --> AI\["Application Insights"]

