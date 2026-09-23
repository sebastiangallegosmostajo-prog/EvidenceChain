\# AI-REVIEW-01 — Revisión de código asistida



El bloque propuesto no debe ejecutarse. Contiene problemas de seguridad, concurrencia, consistencia y diseño asíncrono.



\## Hallazgos



| Defecto | Severidad | Impacto | Corrección |

|---|---|---|---|

| Método declarado como `async void` | Alta | El llamador no puede esperar su finalización ni capturar excepciones. Una excepción podría finalizar el proceso o quedar fuera del flujo de manejo de errores. | Devolver `Task` y aceptar `CancellationToken`. |

| Uso concurrente del mismo `DbContext` dentro de `Task.WhenAll` | Crítica | `DbContext` no es thread-safe. Puede producir excepciones, corrupción del seguimiento de entidades o resultados impredecibles. | No ejecutar operaciones paralelas sobre la misma instancia. Procesar mediante un caso de uso transaccional controlado. |

| `SaveChangesAsync` dentro de cada iteración | Alta | Produce múltiples transacciones, más viajes a la base de datos y un estado parcialmente actualizado si una operación falla. | Realizar una única persistencia dentro del caso de uso o una transacción explícita. |

| Aceptación masiva sin validar actor destinatario | Crítica | Cualquier ejecución aceptaría transferencias en nombre de otros custodios, violando autorización y reglas del dominio. | Procesar una transferencia concreta y comprobar que el actor sea `ToCustodianId`. |

| Modificación directa de propiedades | Alta | Omite la máquina de estados, vencimiento, reglas del dominio, auditoría y generación del evento de custodia. | Utilizar `CustodyTransfer.Accept(...)` y el handler correspondiente. |

| Sin concurrencia optimista | Alta | Dos procesos podrían aceptar o modificar la misma transferencia sin detectar el conflicto. | Exigir `If-Match`, comparar `rowversion` y devolver `409 Conflict`. |

| Uso de `DateTime.Now` | Media | Depende de la zona horaria del servidor y dificulta pruebas deterministas. | Utilizar `TimeProvider.GetUtcNow()` o `DateTimeOffset.UtcNow`. |

| Sin cancelación | Media | La operación no puede detenerse cuando el cliente cancela la solicitud o la aplicación se apaga. | Propagar `CancellationToken`. |

| SQL construido mediante interpolación | Crítica | Permite inyección SQL mediante el parámetro `name`. | Usar LINQ o parámetros de `FromSqlInterpolated`. |

| Comparación exacta de nombre sin definición funcional | Baja | Puede producir resultados inesperados por mayúsculas, espacios o nombres duplicados. | Definir búsqueda normalizada y, cuando corresponda, utilizar identificadores. |

| Retorno síncrono y materialización inmediata | Media | Bloquea el hilo durante acceso a datos y devuelve `IEnumerable` aunque el resultado ya está materializado. | Utilizar `Task<IReadOnlyList<T>>` y `ToListAsync`. |

| Consulta de solo lectura con tracking | Baja | Consume memoria innecesariamente. | Agregar `AsNoTracking()`. |



\## Corrección propuesta



La aceptación debe representar una acción individual ejecutada por el custodio destinatario:



```csharp

public async Task AcceptTransferAsync(

&#x20;   Guid transferId,

&#x20;   Guid actorId,

&#x20;   byte\[] expectedRowVersion,

&#x20;   CancellationToken cancellationToken)

{

&#x20;   var transfer = await \_db.CustodyTransfers

&#x20;       .SingleOrDefaultAsync(

&#x20;           item => item.Id == transferId,

&#x20;           cancellationToken)

&#x20;       ?? throw new NotFoundException(

&#x20;           $"No se encontró la transferencia {transferId}.");



&#x20;   \_db.Entry(transfer)

&#x20;       .Property(item => item.RowVersion)

&#x20;       .OriginalValue = expectedRowVersion;



&#x20;   transfer.Accept(

&#x20;       actorId,

&#x20;       \_timeProvider.GetUtcNow());



&#x20;   try

&#x20;   {

&#x20;       await \_db.SaveChangesAsync(

&#x20;           cancellationToken);

&#x20;   }

&#x20;   catch (DbUpdateConcurrencyException)

&#x20;   {

&#x20;       throw new ConflictException(

&#x20;           "La transferencia fue modificada por otro usuario.");

&#x20;   }

}

