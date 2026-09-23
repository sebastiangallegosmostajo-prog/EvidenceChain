\# Uso de inteligencia artificial



\## Herramientas utilizadas



Se utilizó ChatGPT como asistente durante el diseño y desarrollo de la solución.



La IA apoyó principalmente en:



\- Evaluación inicial del enunciado.

\- Organización de la solución con Clean Architecture.

\- Generación de estructuras iniciales para handlers, contratos y componentes React.

\- Revisión de errores de compilación, pruebas y ESLint.

\- Diseño de pruebas para concurrencia, idempotencia, rollback y solicitudes obsoletas.

\- Preparación de la documentación técnica y propuesta de arquitectura Azure.



Todo el código sugerido fue revisado, adaptado y ejecutado localmente antes de considerarse terminado.



\## Sugerencia aceptada



Se aceptó utilizar `rowversion` de SQL Server como mecanismo de concurrencia optimista y exponer su representación Base64 mediante `ETag`/`If-Match`.



La sugerencia fue adecuada porque evita bloqueos pesimistas, permite detectar si otra operación modificó la transferencia y facilita devolver un `409 Conflict` comprensible para el frontend.



También se aceptó utilizar `AbortController` en la bandeja de evidencias para cancelar solicitudes anteriores cuando cambian los filtros.



\## Sugerencia rechazada o corregida



Una propuesta inicial trataba todas las excepciones producidas por `fetch` como errores de conexión. Esto incluía `AbortError`, generado al cancelar intencionalmente una solicitud anterior.



Esa sugerencia era inadecuada porque una búsqueda cancelada podía mostrar el mensaje “No fue posible conectarse con la API”, aunque la red funcionara correctamente.



Se corrigió `apiRequest` para volver a lanzar `AbortError`. Los componentes identifican esa excepción y no muestran un error ni actualizan la interfaz con una respuesta obsoleta.



También se añadió una comprobación de `controller.signal.aborted` antes de aplicar los resultados.



\## Revisión especialmente cuidadosa



La lógica de integridad fue la parte revisada con mayor atención. Se comprobó que:



\- El primer evento utilice el hash génesis.

\- La secuencia comience en uno y sea continua.

\- Cada evento incluya el hash anterior.

\- La serialización mantenga un orden determinista.

\- Las fechas se normalicen a UTC.

\- El hash se recalcule con los mismos campos utilizados al crear el evento.

\- La verificación reporte el primer punto de ruptura.



También se revisaron cuidadosamente la idempotencia y la concurrencia, porque un error en esas áreas podría crear transferencias duplicadas o permitir dos respuestas incompatibles sobre la misma solicitud.



\## Límites del uso de IA



La IA no tuvo acceso autónomo al ambiente local de ejecución. Las conexiones a SQL Server, migraciones, pruebas, puertos Docker, credenciales mediante User Secrets y comportamiento de la interfaz fueron verificados mediante comandos y resultados ejecutados localmente.



Las sugerencias de IA no se consideraron evidencia suficiente hasta superar compilación, pruebas automatizadas y lint.

