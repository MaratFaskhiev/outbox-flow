namespace OutboxFlow.Sample.Models;

internal sealed class SampleTextModel
{
    public SampleTextModel(string value)
    {
        Value = value;
    }

    public string Value { get; }
}