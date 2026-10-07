import { useEffect, useState } from 'react';

type Track = {
  id: number;
  title: string;
  artist: string;
  album: string;
  durationSeconds: number;
};

const DUPLICATE_MESSAGE = 'This track is already in your playlist.';

export default function App() {
  const [tracks, setTracks] = useState<Track[] | null>(null);
  const [playlist, setPlaylist] = useState<Track[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [addMessage, setAddMessage] = useState<string | null>(null);

  useEffect(() => {
    const load = async (url: string) => {
      const res = await fetch(url);
      if (!res.ok) throw new Error(`API returned ${res.status}`);
      return (await res.json()) as Track[];
    };
    Promise.all([load('/api/tracks'), load('/api/playlist')])
      .then(([t, p]) => {
        setTracks(t);
        setPlaylist(p);
      })
      .catch((err: Error) => setError(err.message));
  }, []);

  async function addTrack(track: Track) {
    setAddMessage(null);
    try {
      const res = await fetch('/api/playlist/tracks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ trackId: track.id }),
      });
      if (res.status === 409) {
        setAddMessage(DUPLICATE_MESSAGE);
      } else if (res.ok) {
        const added = (await res.json()) as Track;
        setPlaylist((current) => [...(current ?? []), added]);
      } else {
        setAddMessage(`Could not add "${track.title}" (error ${res.status}).`);
      }
    } catch {
      setAddMessage(`Could not add "${track.title}".`);
    }
  }

  return (
    <main>
      <h1>Music Catalog</h1>
      {error && <p role="alert">Could not reach the API: {error}</p>}
      {!error && (!tracks || !playlist) && <p>Loading…</p>}
      {tracks && playlist && (
        <>
          {addMessage && <p role="alert">{addMessage}</p>}
          <section aria-labelledby="catalog-heading">
            <h2 id="catalog-heading">Catalog</h2>
            <ul>
              {tracks.map((track) => (
                <li key={track.id}>
                  {track.title} · {track.artist} · {track.album}{' '}
                  <button type="button" onClick={() => addTrack(track)}>
                    Add {track.title} to playlist
                  </button>
                </li>
              ))}
            </ul>
          </section>
          <section aria-labelledby="playlist-heading">
            <h2 id="playlist-heading">Playlist</h2>
            {playlist.length === 0 ? (
              <p>Your playlist is empty. Add a track to get started.</p>
            ) : (
              <ul aria-label="Playlist tracks">
                {playlist.map((track) => (
                  <li key={track.id}>
                    {track.title} · {track.artist}
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </main>
  );
}
