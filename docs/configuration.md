# Configuration Reference

> Auto-generated from compiled assemblies. Run `scripts\generate-config-docs.ps1` to regenerate.

## Table of Contents

### OutboxFlow
- [OutboxFlow.Clock](#OutboxFlow-Clock)
- [OutboxFlow.Configuration.IOutboxBuilder](#OutboxFlow-Configuration-IOutboxBuilder)
- [OutboxFlow.Configuration.IPipelineStepBuilder`2](#OutboxFlow-Configuration-IPipelineStepBuilder-2)
- [OutboxFlow.Configuration.OutboxBuilder](#OutboxFlow-Configuration-OutboxBuilder)
- [OutboxFlow.Configuration.ServiceCollectionExtensions](#OutboxFlow-Configuration-ServiceCollectionExtensions)
- [OutboxFlow.Consume.Configuration.ConsumePipelineBuilder](#OutboxFlow-Consume-Configuration-ConsumePipelineBuilder)
- [OutboxFlow.Consume.Configuration.ConsumePipelineStepBuilder`2](#OutboxFlow-Consume-Configuration-ConsumePipelineStepBuilder-2)
- [OutboxFlow.Consume.Configuration.ConsumePipelineStepBuilderExtensions](#OutboxFlow-Consume-Configuration-ConsumePipelineStepBuilderExtensions)
- [OutboxFlow.Consume.Configuration.ConsumerBuilder](#OutboxFlow-Consume-Configuration-ConsumerBuilder)
- [OutboxFlow.Consume.Configuration.ConsumerBuilderExtensions](#OutboxFlow-Consume-Configuration-ConsumerBuilderExtensions)
- [OutboxFlow.Consume.Configuration.IConsumePipelineBuilder](#OutboxFlow-Consume-Configuration-IConsumePipelineBuilder)
- [OutboxFlow.Consume.Configuration.IConsumePipelineStepBuilder`2](#OutboxFlow-Consume-Configuration-IConsumePipelineStepBuilder-2)
- [OutboxFlow.Consume.Configuration.IConsumerBuilder](#OutboxFlow-Consume-Configuration-IConsumerBuilder)
- [OutboxFlow.Consume.ConsumeContext](#OutboxFlow-Consume-ConsumeContext)
- [OutboxFlow.Consume.ConsumePipelineRegistry](#OutboxFlow-Consume-ConsumePipelineRegistry)
- [OutboxFlow.Consume.IConsumeAsyncMiddleware`2](#OutboxFlow-Consume-IConsumeAsyncMiddleware-2)
- [OutboxFlow.Consume.IConsumeContext](#OutboxFlow-Consume-IConsumeContext)
- [OutboxFlow.Consume.IConsumePipelineRegistry](#OutboxFlow-Consume-IConsumePipelineRegistry)
- [OutboxFlow.Consume.IConsumeSyncMiddleware`2](#OutboxFlow-Consume-IConsumeSyncMiddleware-2)
- [OutboxFlow.Consume.IOutboxConsumer](#OutboxFlow-Consume-IOutboxConsumer)
- [OutboxFlow.Consume.OutboxConsumer](#OutboxFlow-Consume-OutboxConsumer)
- [OutboxFlow.Consume.OutboxConsumeResult](#OutboxFlow-Consume-OutboxConsumeResult)
- [OutboxFlow.Consume.OutboxConsumerService](#OutboxFlow-Consume-OutboxConsumerService)
- [OutboxFlow.IClock](#OutboxFlow-IClock)
- [OutboxFlow.IPipelineStep`2](#OutboxFlow-IPipelineStep-2)
- [OutboxFlow.Middleware.IAsyncMiddleware`3](#OutboxFlow-Middleware-IAsyncMiddleware-3)
- [OutboxFlow.Middleware.ISyncMiddleware`3](#OutboxFlow-Middleware-ISyncMiddleware-3)
- [OutboxFlow.Pipeline`2](#OutboxFlow-Pipeline-2)
- [OutboxFlow.PipelineStep`3](#OutboxFlow-PipelineStep-3)
- [OutboxFlow.Produce.Configuration.IProducePipelineBuilder`1](#OutboxFlow-Produce-Configuration-IProducePipelineBuilder-1)
- [OutboxFlow.Produce.Configuration.IProducePipelineStepBuilder`2](#OutboxFlow-Produce-Configuration-IProducePipelineStepBuilder-2)
- [OutboxFlow.Produce.Configuration.IProducerBuilder](#OutboxFlow-Produce-Configuration-IProducerBuilder)
- [OutboxFlow.Produce.Configuration.ProducePipelineBuilder`1](#OutboxFlow-Produce-Configuration-ProducePipelineBuilder-1)
- [OutboxFlow.Produce.Configuration.ProducePipelineStepBuilder`2](#OutboxFlow-Produce-Configuration-ProducePipelineStepBuilder-2)
- [OutboxFlow.Produce.Configuration.ProducePipelineStepBuilderExtensions](#OutboxFlow-Produce-Configuration-ProducePipelineStepBuilderExtensions)
- [OutboxFlow.Produce.Configuration.ProducerBuilder](#OutboxFlow-Produce-Configuration-ProducerBuilder)
- [OutboxFlow.Produce.IProduceAsyncMiddleware`2](#OutboxFlow-Produce-IProduceAsyncMiddleware-2)
- [OutboxFlow.Produce.IProduceContext](#OutboxFlow-Produce-IProduceContext)
- [OutboxFlow.Produce.IProducePipelineRegistry](#OutboxFlow-Produce-IProducePipelineRegistry)
- [OutboxFlow.Produce.IProducer](#OutboxFlow-Produce-IProducer)
- [OutboxFlow.Produce.IProduceSyncMiddleware`2](#OutboxFlow-Produce-IProduceSyncMiddleware-2)
- [OutboxFlow.Produce.ProduceContext](#OutboxFlow-Produce-ProduceContext)
- [OutboxFlow.Produce.ProducePipelineRegistry](#OutboxFlow-Produce-ProducePipelineRegistry)
- [OutboxFlow.Produce.Producer](#OutboxFlow-Produce-Producer)
- [OutboxFlow.Serialization.ISerializer`1](#OutboxFlow-Serialization-ISerializer-1)
- [OutboxFlow.Serialization.JsonSerializer](#OutboxFlow-Serialization-JsonSerializer)
- [OutboxFlow.Serialization.ProducePipelineStepBuilderExtensions](#OutboxFlow-Serialization-ProducePipelineStepBuilderExtensions)
- [OutboxFlow.Storage.Configuration.IOutboxStorageRegistrar](#OutboxFlow-Storage-Configuration-IOutboxStorageRegistrar)
- [OutboxFlow.Storage.Configuration.OutboxStorageConsumerOptions](#OutboxFlow-Storage-Configuration-OutboxStorageConsumerOptions)
- [OutboxFlow.Storage.IDbConnectionFactory](#OutboxFlow-Storage-IDbConnectionFactory)
- [OutboxFlow.Storage.IOutboxLock](#OutboxFlow-Storage-IOutboxLock)
- [OutboxFlow.Storage.IOutboxLockManager](#OutboxFlow-Storage-IOutboxLockManager)
- [OutboxFlow.Storage.IOutboxMessage](#OutboxFlow-Storage-IOutboxMessage)
- [OutboxFlow.Storage.IOutboxStorage](#OutboxFlow-Storage-IOutboxStorage)

### OutboxFlow.Kafka
- [OutboxFlow.Kafka.ConsumePipelineStepBuilderExtensions](#OutboxFlow-Kafka-ConsumePipelineStepBuilderExtensions)
- [OutboxFlow.Kafka.DefaultKafkaProducerBuilder](#OutboxFlow-Kafka-DefaultKafkaProducerBuilder)
- [OutboxFlow.Kafka.IKafkaProducerBuilder](#OutboxFlow-Kafka-IKafkaProducerBuilder)
- [OutboxFlow.Kafka.IKafkaProducerRegistry](#OutboxFlow-Kafka-IKafkaProducerRegistry)
- [OutboxFlow.Kafka.KafkaProducerRegistry](#OutboxFlow-Kafka-KafkaProducerRegistry)
- [OutboxFlow.Kafka.ProducerConfigExtensions](#OutboxFlow-Kafka-ProducerConfigExtensions)
- [OutboxFlow.Kafka.ServiceCollectionExtensions](#OutboxFlow-Kafka-ServiceCollectionExtensions)

### OutboxFlow.Postgres
- [OutboxFlow.Postgres.ConsumerBuilderExtensions](#OutboxFlow-Postgres-ConsumerBuilderExtensions)
- [OutboxFlow.Postgres.ConsumerOutboxStorageRegistrar](#OutboxFlow-Postgres-ConsumerOutboxStorageRegistrar)
- [OutboxFlow.Postgres.DefaultDbConnectionFactory](#OutboxFlow-Postgres-DefaultDbConnectionFactory)
- [OutboxFlow.Postgres.OutboxLock](#OutboxFlow-Postgres-OutboxLock)
- [OutboxFlow.Postgres.OutboxLockManager](#OutboxFlow-Postgres-OutboxLockManager)
- [OutboxFlow.Postgres.OutboxMessage](#OutboxFlow-Postgres-OutboxMessage)
- [OutboxFlow.Postgres.OutboxStorage](#OutboxFlow-Postgres-OutboxStorage)
- [OutboxFlow.Postgres.ProducerBuilderExtensions](#OutboxFlow-Postgres-ProducerBuilderExtensions)
- [OutboxFlow.Postgres.ProducerOutboxStorageRegistrar](#OutboxFlow-Postgres-ProducerOutboxStorageRegistrar)

---

# Configuration Reference

> Auto-generated from compiled assemblies. Run `scripts\generate-config-docs.ps1` to regenerate.

## OutboxFlow

### OutboxFlow.Clock

- **Implements:** IClock

#### Properties

- **UtcNow** : `DateTime`

#### Methods

##### Delay

- **Return type:** `Task`
- **Parameters:**
  - `delay` (`TimeSpan`)
  - `cancellationToken` (`CancellationToken`)


### OutboxFlow.Consume.ConsumeContext

- **Implements:** IConsumeContext

#### Properties

- **CancellationToken** : `CancellationToken`
- **ServiceProvider** : `IServiceProvider`


### OutboxFlow.Consume.Configuration.ConsumePipelineBuilder

- **Implements:** IConsumePipelineBuilder, IConsumePipelineStepBuilder`2, IPipelineStepBuilder`2

#### Methods

##### AddAsyncStep

- **Return type:** `IConsumePipelineStepBuilder<IOutboxMessage, TOut>`
- **Parameters:**
  - `action` (`Func<IOutboxMessage, IConsumeContext, ValueTask<TOut>>`)

##### AddSyncStep

- **Return type:** `IConsumePipelineStepBuilder<IOutboxMessage, TOut>`
- **Parameters:**
  - `action` (`Func<IOutboxMessage, IConsumeContext, TOut>`)

##### Build

- **Return type:** `IPipelineStep<IConsumeContext, IOutboxMessage>`
- **Parameters:** none


### OutboxFlow.Consume.ConsumePipelineRegistry

- **Implements:** IConsumePipelineRegistry

#### Methods

##### GetPipeline

- **Return type:** `IPipelineStep<IConsumeContext, IOutboxMessage>`
- **Parameters:**
  - `destination` (`String`)

##### GetPipeline

- **Return type:** `IPipelineStep<IConsumeContext, IOutboxMessage>`
- **Parameters:** none


### OutboxFlow.Consume.Configuration.ConsumePipelineStepBuilder`2

- **Implements:** IConsumePipelineStepBuilder`2, IPipelineStepBuilder`2
- **Description:** Outbox consume pipeline step builder.

#### Methods

##### AddAsyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IConsumeContext, ValueTask<TNext>>`)

##### AddSyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IConsumeContext, TNext>`)

##### Build

- **Return type:** `IPipelineStep<IConsumeContext, TIn>`
- **Parameters:** none


### OutboxFlow.Consume.Configuration.ConsumerBuilder

- **Implements:** IConsumerBuilder

#### Properties

- **BatchSize** : `Int32`
- **ConsumeDelay** : `TimeSpan`
- **IsolationLevel** : `IsolationLevel`
- **OutboxStorageRegistrar** : `IOutboxStorageRegistrar`
- **Timeout** : `TimeSpan`

#### Methods

##### AddRoute

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `destination` (`String`)
  - `configure` (`Action<IConsumePipelineBuilder>`)

##### Build

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)

##### SetDefaultRoute

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `configure` (`Action<IConsumePipelineBuilder>`)


### OutboxFlow.Middleware.IAsyncMiddleware`3

- **Description:** Represents an asynchronous middleware.

#### Methods

##### RunAsync

- **Return type:** `ValueTask<TOut>`
- **Parameters:**
  - `message` (`TIn`)
  - `context` (`TContext`)
- **Description:** Runs a middleware.


### OutboxFlow.IClock

- **Description:** Provides date and time functions.

#### Properties

- **UtcNow** : `DateTime` — Gets the current date and time.

#### Methods

##### Delay

- **Return type:** `Task`
- **Parameters:**
  - `delay` (`TimeSpan`)
  - `cancellationToken` (`CancellationToken`)


### OutboxFlow.Consume.IConsumeAsyncMiddleware`2

- **Implements:** IAsyncMiddleware`3
- **Description:** Represents a consume middleware.


### OutboxFlow.Consume.IConsumeContext

- **Description:** Context for the outbox consume operation.

#### Properties

- **CancellationToken** : `CancellationToken` — Gets the cancellation token.
- **ServiceProvider** : `IServiceProvider` — Gets the service provider.


### OutboxFlow.Consume.Configuration.IConsumePipelineBuilder

- **Implements:** IConsumePipelineStepBuilder`2, IPipelineStepBuilder`2
- **Description:** Represents a consume pipeline builder.


### OutboxFlow.Consume.IConsumePipelineRegistry

- **Description:** Consume pipeline registry.

#### Methods

##### GetPipeline

- **Return type:** `IPipelineStep<IConsumeContext, IOutboxMessage>`
- **Parameters:**
  - `destination` (`String`)
- **Description:** Gets a consume pipeline for the specified destination.

##### GetPipeline

- **Return type:** `IPipelineStep<IConsumeContext, IOutboxMessage>`
- **Parameters:** none
- **Description:** Gets a consume pipeline for the specified destination.


### OutboxFlow.Consume.Configuration.IConsumePipelineStepBuilder`2

- **Implements:** IPipelineStepBuilder`2
- **Description:** Represents a consume pipeline step builder.

#### Methods

##### AddAsyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IConsumeContext, ValueTask<TNext>>`)
- **Description:** Adds an asynchronous step to the pipeline.

##### AddSyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IConsumeContext, TNext>`)
- **Description:** Adds a synchronous step to the pipeline.


### OutboxFlow.Consume.Configuration.IConsumerBuilder

- **Description:** Builds an outbox consumer.

#### Properties

- **BatchSize** : `Int32` — Gets or sets the amount of messages to consume.
- **ConsumeDelay** : `TimeSpan` — Gets or sets the delay between each attempt to consume messages.
- **IsolationLevel** : `IsolationLevel` — Gets or sets the transaction isolation level.
- **OutboxStorageRegistrar** : `IOutboxStorageRegistrar` — Gets or sets the registrar to register an outbox storage.
- **Timeout** : `TimeSpan` — Gets or sets the consume operation timeout.

#### Methods

##### AddRoute

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `destination` (`String`)
  - `configure` (`Action<IConsumePipelineBuilder>`)
- **Description:** Configures consume pipeline for the specified destination.

##### Build

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Builds an outbox consumer.

##### SetDefaultRoute

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `configure` (`Action<IConsumePipelineBuilder>`)
- **Description:** Configures the default consume pipeline.


### OutboxFlow.Consume.IConsumeSyncMiddleware`2

- **Implements:** ISyncMiddleware`3
- **Description:** Represents an asynchronous synchronous consume middleware.


### OutboxFlow.Storage.IDbConnectionFactory


#### Methods

##### Create

- **Return type:** `IDbConnection`
- **Parameters:** none


### OutboxFlow.Configuration.IOutboxBuilder

- **Description:** Builds outbox pipelines.

#### Methods

##### AddConsumer

- **Return type:** `IOutboxBuilder`
- **Parameters:**
  - `configure` (`Action<IConsumerBuilder>`)
- **Description:** Configures outbox consume pipelines.

##### AddProducer

- **Return type:** `IOutboxBuilder`
- **Parameters:**
  - `configure` (`Action<IProducerBuilder>`)
- **Description:** Configures outbox produce pipelines.

##### Build

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Builds outbox pipelines.


### OutboxFlow.Consume.IOutboxConsumer

- **Description:** Consumes stored outbox messages.

#### Methods

##### ConsumeAsync

- **Return type:** `ValueTask<OutboxConsumeResult>`
- **Parameters:**
  - `cancellationToken` (`CancellationToken`)
- **Description:** Consumes stored outbox messages.


### OutboxFlow.Storage.IOutboxLock

- **Description:** Outbox lock.

#### Properties

- **ExpireAt** : `DateTime` — Gets the lock expiration date and time.
- **Id** : `Guid` — Gets the lock ID.


### OutboxFlow.Storage.IOutboxLockManager

- **Description:** Outbox lock manager.

#### Methods

##### LockAsync

- **Return type:** `ValueTask<IOutboxLock>`
- **Parameters:**
  - `lockTimeout` (`TimeSpan`)
  - `cancellationToken` (`CancellationToken`)
- **Description:** Locks the outbox.

##### ReleaseAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `outboxLock` (`IOutboxLock`)
  - `cancellationToken` (`CancellationToken`)
- **Description:** Releases the outbox lock.


### OutboxFlow.Storage.IOutboxMessage

- **Description:** Outbox message.

#### Properties

- **Destination** : `String` — Gets the destination.
- **Headers** : `IDictionary<String, String>` — Gets the message headers.
- **Key** : `Byte[]` — Gets the message key.
- **Value** : `Byte[]` — Gets the message value.


### OutboxFlow.Storage.IOutboxStorage

- **Description:** Outbox message storage.

#### Methods

##### DeleteAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `outboxMessages` (`IReadOnlyCollection<IOutboxMessage>`)
  - `cancellationToken` (`CancellationToken`)
- **Description:** Deletes outbox messages from the storage.

##### FetchAsync

- **Return type:** `ValueTask<IReadOnlyCollection<IOutboxMessage>>`
- **Parameters:**
  - `batchSize` (`Int32`)
  - `cancellationToken` (`CancellationToken`)
- **Description:** Fetches outbox messages from the storage.

##### SaveAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `context` (`IProduceContext`)
- **Description:** Saves an outbox message to the storage.

##### SaveBatchAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `contexts` (`IReadOnlyCollection<IProduceContext>`)
- **Description:** Saves multiple outbox messages to the storage in a single operation.


### OutboxFlow.Storage.Configuration.IOutboxStorageRegistrar

- **Description:** Registers an outbox storage.

#### Methods

##### Register

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Registers an outbox storage.


### OutboxFlow.IPipelineStep`2

- **Description:** Represents a pipeline step.

#### Methods

##### RunAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `message` (`T`)
  - `context` (`TContext`)
- **Description:** Runs a pipeline step.


### OutboxFlow.Configuration.IPipelineStepBuilder`2

- **Description:** Represents a pipeline step builder.

#### Methods

##### Build

- **Return type:** `IPipelineStep<TContext, TIn>`
- **Parameters:** none
- **Description:** Builds a produce pipeline step.


### OutboxFlow.Produce.IProduceAsyncMiddleware`2

- **Implements:** IAsyncMiddleware`3
- **Description:** Represents an asynchronous produce middleware.


### OutboxFlow.Produce.IProduceContext

- **Description:** Context for the outbox produce operation.

#### Properties

- **CancellationToken** : `CancellationToken` — Gets the cancellation token.
- **Destination** : `String` — Gets or sets the destination.
- **Headers** : `IDictionary<String, String>` — Gets the message headers.
- **Key** : `Byte[]` — Gets or sets the message key.
- **ServiceProvider** : `IServiceProvider` — Gets the service provider.
- **Value** : `Byte[]` — Gets or sets the message value.


### OutboxFlow.Produce.Configuration.IProducePipelineBuilder`1

- **Implements:** IProducePipelineStepBuilder`2, IPipelineStepBuilder`2
- **Description:** Outbox produce pipeline builder.


### OutboxFlow.Produce.IProducePipelineRegistry

- **Description:** Produce pipeline registry.

#### Methods

##### GetPipeline

- **Return type:** `IPipelineStep<IProduceContext, T>`
- **Parameters:** none
- **Description:** Gets the pipeline by the message type.


### OutboxFlow.Produce.Configuration.IProducePipelineStepBuilder`2

- **Implements:** IPipelineStepBuilder`2
- **Description:** Represents a produce pipeline step builder.

#### Methods

##### AddAsyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IProduceContext, ValueTask<TNext>>`)
- **Description:** Adds an asynchronous step to the pipeline.

##### AddSyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IProduceContext, TNext>`)
- **Description:** Adds a synchronous step to the pipeline.


### OutboxFlow.Produce.IProducer

- **Description:** Produces an outbox message.

#### Methods

##### ProduceAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `message` (`T`)
  - `cancellationToken` (`CancellationToken`)
- **Description:** Produces an outbox message.


### OutboxFlow.Produce.Configuration.IProducerBuilder

- **Description:** Builds an outbox producer.

#### Properties

- **OutboxStorageRegistrar** : `IOutboxStorageRegistrar` — Gets or sets the registrar to register an outbox storage.

#### Methods

##### Build

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Builds an outbox producer.

##### ForMessage

- **Return type:** `IProducerBuilder`
- **Parameters:**
  - `configure` (`Action<IProducePipelineBuilder<T>>`)
- **Description:** Configures produce pipeline for the specified message type.


### OutboxFlow.Produce.IProduceSyncMiddleware`2

- **Implements:** ISyncMiddleware`3
- **Description:** Represents a synchronous produce middleware.


### OutboxFlow.Serialization.ISerializer`1

- **Description:** Serializes values.

#### Methods

##### Serialize

- **Return type:** `T`
- **Parameters:**
  - `value` (`TValue`)
- **Description:** Serializes the specified value.


### OutboxFlow.Middleware.ISyncMiddleware`3

- **Description:** Represents a synchronous middleware.

#### Methods

##### Run

- **Return type:** `TOut`
- **Parameters:**
  - `message` (`TIn`)
  - `context` (`TContext`)
- **Description:** Runs a middleware.


### OutboxFlow.Serialization.JsonSerializer

- **Implements:** ISerializer`1
- **Description:** Serializes values to a JSON string, encoded as UTF-8 bytes.

#### Methods

##### Serialize

- **Return type:** `Byte[]`
- **Parameters:**
  - `value` (`TValue`)
- **Description:** Serializes the specified value to a JSON string, encoded as UTF-8 bytes.


### OutboxFlow.Configuration.OutboxBuilder

- **Implements:** IOutboxBuilder

#### Methods

##### AddConsumer

- **Return type:** `IOutboxBuilder`
- **Parameters:**
  - `configure` (`Action<IConsumerBuilder>`)

##### AddProducer

- **Return type:** `IOutboxBuilder`
- **Parameters:**
  - `configure` (`Action<IProducerBuilder>`)

##### Build

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)


### OutboxFlow.Consume.OutboxConsumer

- **Implements:** IOutboxConsumer

#### Methods

##### ConsumeAsync

- **Return type:** `ValueTask<OutboxConsumeResult>`
- **Parameters:**
  - `cancellationToken` (`CancellationToken`)


### OutboxFlow.Consume.OutboxConsumeResult

- **Implements:** IEquatable`1
- **Description:** Contains result of consume operation.

#### Properties

- **Count** : `Int32` — The amount of consumed messages, if messages were successfully consumed.
- **IsSuccessful** : `Boolean`


### OutboxFlow.Consume.OutboxConsumerService

- **Base type:** `Microsoft.Extensions.Hosting.BackgroundService`
- **Implements:** IHostedService
- **Description:** Background service which consumes stored outbox messages.

#### Properties

- **ExecuteTask** : `Task`

#### Methods

##### Dispose

- **Return type:** `Void`
- **Parameters:** none

##### StartAsync

- **Return type:** `Task`
- **Parameters:**
  - `cancellationToken` (`CancellationToken`)

##### StopAsync

- **Return type:** `Task`
- **Parameters:**
  - `cancellationToken` (`CancellationToken`)


### OutboxFlow.Storage.Configuration.OutboxStorageConsumerOptions


#### Properties

- **BatchSize** : `Int32` — Gets or sets the amount of messages to consume.
- **ConsumeDelay** : `TimeSpan` — Gets or sets the delay between each attempt to consume messages.
- **IsolationLevel** : `IsolationLevel` — Gets or sets the transaction isolation level.
- **Timeout** : `TimeSpan` — Get or sets the consume operation timeout.


### OutboxFlow.Pipeline`2

- **Implements:** IPipelineStep`2
- **Description:** Outbox pipeline.

#### Methods

##### RunAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `message` (`T`)
  - `context` (`TContext`)


### OutboxFlow.PipelineStep`3

- **Implements:** IPipelineStep`2
- **Description:** Outbox pipeline step.

#### Methods

##### RunAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `message` (`TIn`)
  - `context` (`TContext`)


### OutboxFlow.Produce.ProduceContext

- **Implements:** IProduceContext

#### Properties

- **CancellationToken** : `CancellationToken`
- **Destination** : `String`
- **Headers** : `IDictionary<String, String>`
- **Key** : `Byte[]`
- **ServiceProvider** : `IServiceProvider`
- **Value** : `Byte[]`


### OutboxFlow.Produce.Configuration.ProducePipelineBuilder`1

- **Implements:** IProducePipelineBuilder`1, IProducePipelineStepBuilder`2, IPipelineStepBuilder`2

#### Methods

##### AddAsyncStep

- **Return type:** `IProducePipelineStepBuilder<T, TOut>`
- **Parameters:**
  - `action` (`Func<T, IProduceContext, ValueTask<TOut>>`)

##### AddSyncStep

- **Return type:** `IProducePipelineStepBuilder<T, TOut>`
- **Parameters:**
  - `action` (`Func<T, IProduceContext, TOut>`)

##### Build

- **Return type:** `IPipelineStep<IProduceContext, T>`
- **Parameters:** none


### OutboxFlow.Produce.ProducePipelineRegistry

- **Implements:** IProducePipelineRegistry

#### Methods

##### GetPipeline

- **Return type:** `IPipelineStep<IProduceContext, T>`
- **Parameters:** none


### OutboxFlow.Produce.Configuration.ProducePipelineStepBuilder`2

- **Implements:** IProducePipelineStepBuilder`2, IPipelineStepBuilder`2
- **Description:** Outbox produce pipeline step builder.

#### Methods

##### AddAsyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IProduceContext, ValueTask<TNext>>`)
- **Description:** Adds a step to the pipeline.

##### AddSyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `action` (`Func<TOut, IProduceContext, TNext>`)
- **Description:** Adds a synchronous step to the pipeline.

##### Build

- **Return type:** `IPipelineStep<IProduceContext, TIn>`
- **Parameters:** none


### OutboxFlow.Produce.Producer

- **Implements:** IProducer

#### Methods

##### ProduceAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `message` (`T`)
  - `cancellationToken` (`CancellationToken`)


### OutboxFlow.Produce.Configuration.ProducerBuilder

- **Implements:** IProducerBuilder

#### Properties

- **OutboxStorageRegistrar** : `IOutboxStorageRegistrar`

#### Methods

##### Build

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)

##### ForMessage

- **Return type:** `IProducerBuilder`
- **Parameters:**
  - `configure` (`Action<IProducePipelineBuilder<T>>`)


### Extension Methods

#### AddAsyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IConsumePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds an asynchronous middleware to the pipeline.

#### AddAsyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `step` (`IConsumePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds an asynchronous middleware to the pipeline.

#### AddSyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IConsumePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds a synchronous middleware to the pipeline.

#### AddSyncStep

- **Return type:** `IConsumePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `step` (`IConsumePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds a synchronous middleware to the pipeline.

#### SetBatchSize

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `consumerBuilder` (`IConsumerBuilder`)
  - `batchSize` (`Int32`)
- **Description:** Sets the amount of messages to consume.

#### SetConsumeDelay

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `consumerBuilder` (`IConsumerBuilder`)
  - `consumeDelay` (`TimeSpan`)
- **Description:** Sets the delay between each attempt to consume messages.

#### SetIsolationLevel

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `consumerBuilder` (`IConsumerBuilder`)
  - `isolationLevel` (`IsolationLevel`)
- **Description:** Sets the transaction isolation level.

#### SetOutboxStorageRegistrar

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `consumerBuilder` (`IConsumerBuilder`)
  - `registrar` (`IOutboxStorageRegistrar`)
- **Description:** Sets the registrar to register an outbox storage.

#### SetTimeout

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `consumerBuilder` (`IConsumerBuilder`)
  - `timeout` (`TimeSpan`)
- **Description:** Sets the consume operation timeout.

#### AddAsyncStep

- **Return type:** `IProducePipelineStepBuilder<T, T>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<T, T>`)
- **Description:** Adds an asynchronous middleware to the pipeline.

#### AddAsyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds an asynchronous middleware to the pipeline.

#### AddAsyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds an asynchronous middleware to the pipeline.

#### AddSyncStep

- **Return type:** `IProducePipelineStepBuilder<T, T>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<T, T>`)
- **Description:** Adds a synchronous middleware to the pipeline.

#### AddSyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds a synchronous middleware to the pipeline.

#### AddSyncStep

- **Return type:** `IProducePipelineStepBuilder<TOut, TNext>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Adds a synchronous middleware to the pipeline.

#### ForEach

- **Return type:** `IProducePipelineStepBuilder<IReadOnlyCollection<TItem>, IReadOnlyCollection<IProduceContext>>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<IReadOnlyCollection<TItem>, IReadOnlyCollection<TItem>>`)
  - `configure` (`Action<IProducePipelineBuilder<TItem>>`)
- **Description:** Iterates over each item in the collection, runs the configured sub-pipeline for each,
            and collects the resulting produce contexts.

#### Save

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Saves the message to the outbox storage.

#### SaveBatch

- **Return type:** `IProducePipelineStepBuilder<IReadOnlyCollection<IProduceContext>, IReadOnlyCollection<IProduceContext>>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<IReadOnlyCollection<TSource>, IReadOnlyCollection<IProduceContext>>`)
- **Description:** Saves all collected produce contexts to the outbox storage in a single batch operation.

#### SetDestination

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
  - `destination` (`String`)
- **Description:** Sets the message destination.

#### SetKey

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
  - `keyProvider` (`Func<TOut, Byte[]>`)
- **Description:** Sets the message key.

#### Serialize

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Serialize a message by using the specified serializer.

#### SerializeKey

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
  - `keyProvider` (`Func<TOut, TKey>`)
- **Description:** Serialize a message key by using the specified serializer.

#### SerializeKeyWithJson

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
  - `keyProvider` (`Func<TOut, TKey>`)
- **Description:** Serialize a message key to a JSON string, encoded as UTF-8 bytes.

#### SerializeKeyWithProtobuf

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
  - `keyProvider` (`Func<TOut, TKey>`)
- **Description:** Serialize a message key to a byte array in protobuf encoding.

#### SerializeWithJson

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Serialize a message to a JSON string, encoded as UTF-8 bytes.

#### SerializeWithProtobuf

- **Return type:** `IProducePipelineStepBuilder<TOut, TOut>`
- **Parameters:**
  - `step` (`IProducePipelineStepBuilder<TIn, TOut>`)
- **Description:** Serialize a message to a byte array in protobuf encoding.

#### AddOutbox

- **Return type:** `IServiceCollection`
- **Parameters:**
  - `services` (`IServiceCollection`)
  - `configure` (`Action<IOutboxBuilder>`)
- **Description:** Registers the outbox dependencies.

## OutboxFlow.Postgres

### OutboxFlow.Postgres.ConsumerOutboxStorageRegistrar

- **Implements:** IOutboxStorageRegistrar
- **Description:** Registers an outbox storage based on PostgreSQL.

#### Methods

##### Register

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Registers an outbox storage based on PostgreSQL.


### OutboxFlow.Postgres.DefaultDbConnectionFactory

- **Implements:** IDbConnectionFactory

#### Methods

##### Create

- **Return type:** `IDbConnection`
- **Parameters:** none


### OutboxFlow.Postgres.OutboxLock

- **Implements:** IOutboxLock, IEquatable`1
- **Description:** Represents an outbox lock.

#### Properties

- **ExpireAt** : `DateTime` — Expiration date and time.
- **Id** : `Guid` — Lock ID.


### OutboxFlow.Postgres.OutboxLockManager

- **Implements:** IOutboxLockManager
- **Description:** Outbox lock manager which uses PostgreSQL as an underlying storage.

#### Methods

##### LockAsync

- **Return type:** `ValueTask<IOutboxLock>`
- **Parameters:**
  - `lockTimeout` (`TimeSpan`)
  - `cancellationToken` (`CancellationToken`)

##### ReleaseAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `outboxLock` (`IOutboxLock`)
  - `cancellationToken` (`CancellationToken`)


### OutboxFlow.Postgres.OutboxMessage

- **Implements:** IOutboxMessage

#### Properties

- **Destination** : `String`
- **Headers** : `IDictionary<String, String>`
- **Id** : `Int64` — Gets the message ID.
- **Key** : `Byte[]`
- **Value** : `Byte[]`


### OutboxFlow.Postgres.OutboxStorage

- **Implements:** IOutboxStorage
- **Description:** Outbox message storage which uses PostgreSQL as an underlying storage.

#### Methods

##### DeleteAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `outboxMessages` (`IReadOnlyCollection<IOutboxMessage>`)
  - `cancellationToken` (`CancellationToken`)

##### FetchAsync

- **Return type:** `ValueTask<IReadOnlyCollection<IOutboxMessage>>`
- **Parameters:**
  - `batchSize` (`Int32`)
  - `cancellationToken` (`CancellationToken`)

##### SaveAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `context` (`IProduceContext`)

##### SaveBatchAsync

- **Return type:** `ValueTask`
- **Parameters:**
  - `contexts` (`IReadOnlyCollection<IProduceContext>`)


### OutboxFlow.Postgres.ProducerOutboxStorageRegistrar

- **Implements:** IOutboxStorageRegistrar
- **Description:** Registers an outbox storage based on PostgreSQL.

#### Methods

##### Register

- **Return type:** `Void`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Registers an outbox storage based on PostgreSQL.


### Extension Methods

#### UsePostgres

- **Return type:** `IConsumerBuilder`
- **Parameters:**
  - `builder` (`IConsumerBuilder`)
  - `connectionString` (`String`)
- **Description:** Configures the consumer to use an outbox storage based on PostgreSQL.

#### UsePostgres

- **Return type:** `IProducerBuilder`
- **Parameters:**
  - `builder` (`IProducerBuilder`)
  - `connectionString` (`String`)
- **Description:** Configures the producer to use an outbox storage based on PostgreSQL.

## OutboxFlow.Kafka

### OutboxFlow.Kafka.DefaultKafkaProducerBuilder

- **Implements:** IKafkaProducerBuilder

#### Methods

##### Create

- **Return type:** `IProducer<Byte[], Byte[]>`
- **Parameters:**
  - `producerConfig` (`ProducerConfig`)


### OutboxFlow.Kafka.IKafkaProducerBuilder

- **Description:** Kafka producer builder.

#### Methods

##### Create

- **Return type:** `IProducer<Byte[], Byte[]>`
- **Parameters:**
  - `producerConfig` (`ProducerConfig`)
- **Description:** Creates the Kafka producer for the specified configuration.


### OutboxFlow.Kafka.IKafkaProducerRegistry

- **Description:** Kafka producer registry.

#### Methods

##### GetOrCreate

- **Return type:** `IProducer<Byte[], Byte[]>`
- **Parameters:**
  - `producerBuilder` (`IKafkaProducerBuilder`)
  - `producerConfig` (`ProducerConfig`)
- **Description:** Gets or creates the Kafka producer for the specified configuration.

##### Remove

- **Return type:** `Void`
- **Parameters:**
  - `producerConfig` (`ProducerConfig`)
- **Description:** Removes the Kafka producer for the specified configuration.


### OutboxFlow.Kafka.KafkaProducerRegistry

- **Implements:** IKafkaProducerRegistry

#### Methods

##### Dispose

- **Return type:** `Void`
- **Parameters:** none

##### DisposeAsync

- **Return type:** `ValueTask`
- **Parameters:** none

##### GetOrCreate

- **Return type:** `IProducer<Byte[], Byte[]>`
- **Parameters:**
  - `producerBuilder` (`IKafkaProducerBuilder`)
  - `producerConfig` (`ProducerConfig`)

##### Remove

- **Return type:** `Void`
- **Parameters:**
  - `producerConfig` (`ProducerConfig`)


### Extension Methods

#### SendToKafka

- **Return type:** `IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>`
- **Parameters:**
  - `pipeline` (`IConsumePipelineStepBuilder<TIn, IOutboxMessage>`)
  - `producerConfig` (`ProducerConfig`)
- **Description:** Sends the message to the Kafka topic.

#### SendToKafka

- **Return type:** `IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>`
- **Parameters:**
  - `pipeline` (`IConsumePipelineStepBuilder<TIn, IOutboxMessage>`)
  - `producerConfig` (`ProducerConfig`)
- **Description:** Sends the message to the Kafka topic.

#### ApplyBatchDefaults

- **Return type:** `ProducerConfig`
- **Parameters:**
  - `config` (`ProducerConfig`)

#### ApplyOutboxDefaults

- **Return type:** `ProducerConfig`
- **Parameters:**
  - `config` (`ProducerConfig`)

#### AddKafka

- **Return type:** `IServiceCollection`
- **Parameters:**
  - `services` (`IServiceCollection`)
- **Description:** Registers Apache Kafka dependencies.

