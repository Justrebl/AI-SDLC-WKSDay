public record Track(int Id, string Title, string Artist, string Album, int DurationSeconds);

public class PlaylistStore
{
    private readonly object _gate = new();
    private readonly List<Track> _tracks = [];

    public IReadOnlyList<Track> GetAll()
    {
        lock (_gate) return _tracks.ToList();
    }

    // Returns false when the track is already in the playlist.
    public bool TryAdd(Track track)
    {
        lock (_gate)
        {
            if (_tracks.Any(t => t.Id == track.Id)) return false;
            _tracks.Add(track);
            return true;
        }
    }
}
