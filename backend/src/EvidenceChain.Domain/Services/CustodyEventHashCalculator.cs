using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using EvidenceChain.Domain.Enums;

namespace EvidenceChain.Domain.Services;

public static class CustodyEventHashCalculator
{
    public static string Calculate(
        Guid eventId,
        Guid evidenceId,
        long sequenceNumber,
        CustodyEventType eventType,
        Guid actorId,
        Guid? fromCustodianId,
        Guid? toCustodianId,
        Guid? transferId,
        DateTimeOffset occurredAtUtc,
        string details,
        string previousHash)
    {
        var buffer = new ArrayBufferWriter<byte>();

        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();

            writer.WriteNumber("version", 1);
            writer.WriteString("eventId", eventId.ToString("N"));
            writer.WriteString("evidenceId", evidenceId.ToString("N"));
            writer.WriteNumber("sequenceNumber", sequenceNumber);
            writer.WriteNumber("eventType", (int)eventType);
            writer.WriteString("actorId", actorId.ToString("N"));

            WriteNullableGuid(
                writer,
                "fromCustodianId",
                fromCustodianId);

            WriteNullableGuid(
                writer,
                "toCustodianId",
                toCustodianId);

            WriteNullableGuid(
                writer,
                "transferId",
                transferId);

            writer.WriteString(
                "occurredAtUtc",
                occurredAtUtc
                    .ToUniversalTime()
                    .ToString("O", CultureInfo.InvariantCulture));

            writer.WriteString("details", details);
            writer.WriteString(
                "previousHash",
                previousHash.ToLowerInvariant());

            writer.WriteEndObject();
        }

        var hashBytes = SHA256.HashData(buffer.WrittenSpan);

        return Convert
            .ToHexString(hashBytes)
            .ToLowerInvariant();
    }

    private static void WriteNullableGuid(
        Utf8JsonWriter writer,
        string propertyName,
        Guid? value)
    {
        if (value.HasValue)
        {
            writer.WriteString(
                propertyName,
                value.Value.ToString("N"));
        }
        else
        {
            writer.WriteNull(propertyName);
        }
    }
}