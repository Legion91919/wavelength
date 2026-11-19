using Wavelength.SharedKernel;

namespace Wavelength.Tests.Support;

internal sealed class FakeDirectory : ICatalogueDirectory
{
    private readonly Dictionary<string, TrackRef> _tracks = new()
    {
        ["trk-a"] = new TrackRef("trk-a", "Test Track A", "lbl-one", 200),
        ["trk-b"] = new TrackRef("trk-b", "Test Track B", "lbl-one", 120),
        ["trk-c"] = new TrackRef("trk-c", "Test Track C", "lbl-two", 240),
        ["trk-short"] = new TrackRef("trk-short", "Short Sketch", "lbl-two", 20),
    };

    private readonly Dictionary<string, LabelRef> _labels = new()
    {
        ["lbl-one"] = new LabelRef("lbl-one", "Label One"),
        ["lbl-two"] = new LabelRef("lbl-two", "Label Two"),
    };

    public TrackRef? FindTrack(string trackId) => _tracks.GetValueOrDefault(trackId);

    public LabelRef? FindLabel(string labelId) => _labels.GetValueOrDefault(labelId);

    public IReadOnlyList<LabelRef> Labels() => _labels.Values.ToList();
}

internal sealed class CapturingPublisher : IIntegrationEventPublisher
{
    public List<object> Published { get; } = [];

    public void Publish<TEvent>(TEvent integrationEvent)
        where TEvent : class => Published.Add(integrationEvent);
}
