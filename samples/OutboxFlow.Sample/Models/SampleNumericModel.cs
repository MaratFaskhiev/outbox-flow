namespace OutboxFlow.Sample.Models;

public sealed class SampleNumericModel
{
    public int Value { get; }

    public SampleNumericModel(int value)
    {
        Value = value;
    }
}