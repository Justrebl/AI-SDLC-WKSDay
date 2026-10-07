using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public class PlaylistEmptyStateTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Own class and fixture: nothing here adds, so the playlist stays empty.
    [Fact]
    public async Task Fresh_api_returns_empty_playlist()
    {
        var client = factory.CreateClient();
        var playlist = await client.GetFromJsonAsync<List<TrackDto>>("/api/playlist");
        Assert.NotNull(playlist);
        Assert.Empty(playlist);
    }
}

public class PlaylistTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private HttpClient Client => factory.CreateClient();

    [Fact]
    public async Task Tracks_returns_seed_catalog()
    {
        var tracks = await Client.GetFromJsonAsync<List<TrackDto>>("/api/tracks");
        Assert.NotNull(tracks);
        Assert.Equal(12, tracks.Count);
        Assert.All(tracks, t =>
        {
            Assert.False(string.IsNullOrEmpty(t.Title));
            Assert.False(string.IsNullOrEmpty(t.Artist));
            Assert.False(string.IsNullOrEmpty(t.Album));
            Assert.True(t.DurationSeconds > 0);
        });
    }

    [Fact]
    public async Task Add_returns_201_and_track_appears_in_playlist()
    {
        var client = Client;
        var response = await client.PostAsJsonAsync("/api/playlist/tracks", new { trackId = 1 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var added = await response.Content.ReadFromJsonAsync<TrackDto>();
        Assert.Equal(1, added!.Id);

        var playlist = await client.GetFromJsonAsync<List<TrackDto>>("/api/playlist");
        Assert.Contains(playlist!, t => t.Id == 1);
    }

    [Fact]
    public async Task Duplicate_add_returns_409_and_playlist_is_unchanged()
    {
        var client = Client;
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/playlist/tracks", new { trackId = 2 })).StatusCode);
        var duplicate = await client.PostAsJsonAsync("/api/playlist/tracks", new { trackId = 2 });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var playlist = await client.GetFromJsonAsync<List<TrackDto>>("/api/playlist");
        Assert.Single(playlist!, t => t.Id == 2);
    }

    [Fact]
    public async Task Unknown_track_returns_404_and_playlist_is_unchanged()
    {
        var client = Client;
        var response = await client.PostAsJsonAsync("/api/playlist/tracks", new { trackId = 999 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var playlist = await client.GetFromJsonAsync<List<TrackDto>>("/api/playlist");
        Assert.DoesNotContain(playlist!, t => t.Id == 999);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"trackId\":null}")]
    [InlineData("{\"trackId\":\"abc\"}")]
    [InlineData("not json")]
    public async Task Missing_or_malformed_track_id_returns_400(string body)
    {
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/playlist/tracks", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Concurrent_adds_of_same_track_create_one_entry()
    {
        var client = Client;
        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => client.PostAsJsonAsync("/api/playlist/tracks", new { trackId = 3 })));
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        var playlist = await client.GetFromJsonAsync<List<TrackDto>>("/api/playlist");
        Assert.Single(playlist!, t => t.Id == 3);
    }
}

public record TrackDto(int Id, string Title, string Artist, string Album, int DurationSeconds);
