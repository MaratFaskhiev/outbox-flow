namespace OutboxFlow.Sample.Models;

public sealed class SampleNumericModel
{
    public SampleNumericModel(int value)
    {
        Value = value;
    }

    public int Value { get; }
}