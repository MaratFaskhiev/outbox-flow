using System.Text;
using OutboxFlow.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OutboxFlow.Sample;

internal sealed class CustomSerializer : ISerializer<byte[]>
{
    public byte[] Serialize<TValue>(TValue value)
    {
        if (value is string str) return Encoding.UTF8.GetBytes(str);

        return JsonSerializer.SerializeToUtf8Bytes(value);
    }
}