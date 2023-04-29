namespace OutboxFlow.Sample.Models;

public sealed class SampleTextModel
{
    public string Value { get; }

    public SampleTextModel(string value)
    {
        Value = value;
    }
}