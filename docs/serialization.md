# Serialization Guide

OutboxFlow provides a serialization abstraction for converting messages to byte arrays before saving them to the outbox storage. Built-in serializers support JSON and Protocol Buffers, and you can add custom serializers.

## ISerializer&lt;T&gt;

The core serialization interface:

```csharp
public interface ISerializer<T>
{
    T Serialize<TValue>(TValue value);
}
```

All built-in serializers implement `ISerializer<byte[]>`, producing byte arrays ready for storage.

## JSON Serializer

The `JsonSerializer` uses `System.Text.Json` to serialize messages to UTF-8 encoded JSON.

```csharp
pipeline.SerializeWithJson();
```

To serialize a message key:

```csharp
pipeline.SerializeKeyWithJson(message => message.Id);
```

The JSON serializer is included in the `OutboxFlow` package (no additional dependencies).

## Protobuf Serializer

The protobuf serializer calls `IMessage.ToByteArray()` on Google Protobuf messages.

```csharp
pipeline.SerializeWithProtobuf();
```

To serialize a message key:

```csharp
pipeline.SerializeKeyWithProtobuf(message => message.KeyModel);
```

Requires the message type to implement `Google.Protobuf.IMessage` and the `Google.Protobuf` package.

## Generic Serializer

Use `Serialize<TSerializer>()` with any custom `ISerializer<byte[]>` registered in DI:

```csharp
pipeline.Serialize<MyCustomSerializer, MyModel, byte[]>();
```

To serialize a key:

```csharp
pipeline.SerializeKey<MyCustomSerializer, MyModel, byte[], Guid>(message => message.Id);
```

## Custom Serializer

See `samples/OutboxFlow.Sample/CustomSerializer.cs` for a complete example:

```csharp
<!-- SNIPPET: docs_ser_custom -->
internal sealed class CustomSerializer : ISerializer<byte[]>
{
    public byte[] Serialize<TValue>(TValue value)
    {
        if (value is string str) return Encoding.UTF8.GetBytes(str);

        return JsonSerializer.SerializeToUtf8Bytes(value);
    }
}
<!-- ENDSNIPPET: docs_ser_custom -->
```

### Registration and Usage

Register in DI and use with the generic `Serialize` method:

```csharp
services.AddSingleton<ISerializer<byte[]>, CustomSerializer>();

// In pipeline:
pipeline.Serialize<CustomSerializer, MyModel, byte[]>();
```

### Serializer Extension Methods Summary

| Method | Target | Description |
|---|---|---|
| `SerializeWithJson()` | Produce pipeline | Serialize message value to JSON. |
| `SerializeKeyWithJson(keyProvider)` | Produce pipeline | Serialize message key to JSON. |
| `SerializeWithProtobuf()` | Produce pipeline | Serialize message value to protobuf. |
| `SerializeKeyWithProtobuf(keyProvider)` | Produce pipeline | Serialize message key to protobuf. |
| `Serialize<TSerializer, TIn, TOut>()` | Produce pipeline | Serialize message value with a custom serializer from DI. |
| `SerializeKey<TSerializer, TIn, TOut, TKey>(keyProvider)` | Produce pipeline | Serialize message key with a custom serializer from DI. |
