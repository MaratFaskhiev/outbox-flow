﻿using Moq;
using OutboxFlow.Produce;
using OutboxFlow.Produce.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Produce.Configuration;

public sealed class ProducePipelineStepBuilderTests
{
    [Fact]
    public async Task Build_SyncAction_PipelineRunsAction()
    {
        var isInvoked = false;
        var builder = new ProducePipelineStepBuilder<int, int>((_, _) =>
        {
            isInvoked = true;
            return 1;
        });


        var pipelineStep = builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }

    [Fact]
    public async Task Build_AsyncAction_PipelineRunsAction()
    {
        var isInvoked = false;
        var builder = new ProducePipelineStepBuilder<int, int>((_, _) =>
        {
            isInvoked = true;
            return new ValueTask<int>(1);
        });

        var pipelineStep = builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }

    [Fact]
    public void AddAsyncStep_StepIsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        var builder = new ProducePipelineStepBuilder<int, int>((x, _) => x);

        builder.AddAsyncStep((_, _) => new ValueTask<int>(1));

        Assert.Throws<InvalidOperationException>(() => builder.AddAsyncStep((_, _) => new ValueTask<int>(1)));
    }

    [Fact]
    public void AddSyncStep_StepIsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        var builder = new ProducePipelineStepBuilder<int, int>((x, _) => x);

        builder.AddSyncStep((_, _) => 1);

        Assert.Throws<InvalidOperationException>(() => builder.AddSyncStep((_, _) => 1));
    }

    [Fact]
    public async Task Build_AddSyncStepIsInvoked_PipelineRunsAction()
    {
        var builder = new ProducePipelineStepBuilder<int, int>((x, _) => x);

        var isInvoked = false;
        builder.AddSyncStep((_, _) =>
        {
            isInvoked = true;
            return 1;
        });

        var pipelineStep = builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }

    [Fact]
    public async Task Build_AddAsyncStepIsInvoked_PipelineRunsAction()
    {
        var builder = new ProducePipelineStepBuilder<int, int>((x, _) => x);

        var isInvoked = false;
        builder.AddAsyncStep((_, _) =>
        {
            isInvoked = true;
            return new ValueTask<int>(1);
        });

        var pipelineStep = builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }
}