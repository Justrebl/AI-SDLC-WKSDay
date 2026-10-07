using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PlaylistStore>();

var app = builder.Build();

// Fails fast at startup if the seed file is missing or invalid.
var seedPath = Path.Combine(app.Environment.ContentRootPath, "Data", "tracks.json");
var catalog = JsonSerializer.Deserialize<List<Track>>(
    File.ReadAllText(seedPath),
    new JsonSerializerOptions(JsonSerializerDefaults.Web))
    ?? throw new InvalidOperationException("tracks.json is empty");

app.MapGet("/api/hello", () => new { message = "Hello from the Music Catalog API" });

app.MapGet("/api/tracks", () => catalog);

app.MapGet("/api/playlist", (PlaylistStore store) => store.GetAll());

app.MapPost("/api/playlist/tracks", async (HttpRequest request, PlaylistStore store) =>
{
    int? trackId = null;
    try
    {
        using var doc = await JsonDocument.ParseAsync(request.Body);
        if (doc.RootElement.ValueKind == JsonValueKind.Object
            && doc.RootElement.TryGetProperty("trackId", out var prop)
            && prop.ValueKind == JsonValueKind.Number
            && prop.TryGetInt32(out var parsed))
        {
            trackId = parsed;
        }
    }
    catch (JsonException)
    {
    }

    if (trackId is null)
        return Results.BadRequest(new { error = "trackId is required and must be an integer." });

    var track = catalog.FirstOrDefault(t => t.Id == trackId);
    if (track is null)
        return Results.NotFound(new { error = $"Track {trackId} does not exist." });

    if (!store.TryAdd(track))
        return Results.Conflict(new { error = "This track is already in your playlist." });

    return Results.Created($"/api/playlist/tracks/{track.Id}", track);
});

app.Run();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;