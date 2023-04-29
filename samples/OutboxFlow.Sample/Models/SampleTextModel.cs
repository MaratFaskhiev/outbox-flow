namespace OutboxFlow.Sample.Models;

public sealed class SampleTextModel
{
    public SampleTextModel(string value)
    {
        Value = value;
    }

    public string Value { get; }
}