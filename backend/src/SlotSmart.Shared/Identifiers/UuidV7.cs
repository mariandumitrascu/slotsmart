using System.Buffers.Binary;
using System.Security.Cryptography;

namespace SlotSmart.Shared.Identifiers;

/// <summary>
/// Generates UUIDv7 values per RFC 9562 — Unix-millisecond timestamp prefix + cryptographic randomness.
/// Returns a <see cref="Guid"/>; storage / wire format is the standard 16-byte UUID.
/// </summary>
/// <remarks>
/// Per ADR-007 (dual-key identity pattern): this is the public <c>EntityId</c> of every entity.
/// The storage primary key is a separate <c>bigint</c> surrogate added by EF Core at the Infrastructure layer.
/// </remarks>
public static class UuidV7
{
    /// <summary>Returns a fresh UUIDv7.</summary>
    public static Guid NewGuid() => NewGuid(DateTimeOffset.UtcNow);

    /// <summary>Returns a UUIDv7 stamped with the given timestamp. Visible for testing.</summary>
    public static Guid NewGuid(DateTimeOffset timestamp)
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        var unixMs = timestamp.ToUnixTimeMilliseconds();
        if (unixMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "UUIDv7 requires timestamps after the Unix epoch.");
        }

        // Bytes 0..5: 48-bit big-endian Unix-ms timestamp.
        Span<byte> tsScratch = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(tsScratch, unixMs);
        tsScratch[2..8].CopyTo(bytes[..6]);

        // Byte 6: version 7 in the high nibble; preserve random low nibble.
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

        // Byte 8: RFC 4122 variant (10xx xxxx); preserve random low 6 bits.
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        // Guid's binary layout differs from UUID big-endian; pass bigEndian: true so the Guid
        // round-trips correctly to the standard UUID hex form when serialised.
        return new Guid(bytes, bigEndian: true);
    }
}
