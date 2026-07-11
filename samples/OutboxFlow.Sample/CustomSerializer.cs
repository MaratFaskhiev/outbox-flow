using System.Text;
using OutboxFlow.Serialization;

namespace OutboxFlow.Sample;

public sealed class CustomSerializer : ISerializer<byte[]>
{
    public byte[] Serialize<TValue>(TValue value)
    {
        if (value is string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
    }
}