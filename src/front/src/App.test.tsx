import { fireEvent, render, screen, within } from '@testing-library/react';
import App from './App';

const tracks = [
  { id: 1, title: 'Midnight Static', artist: 'A', album: 'X', durationSeconds: 200 },
  { id: 2, title: 'Green Pipeline', artist: 'B', album: 'Y', durationSeconds: 190 },
];

function stubApi(postStatus: number | ((id: number) => number) = 201) {
  let playlist: typeof tracks = [];
  const fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const json = (status: number, body: unknown) => ({
      ok: status >= 200 && status < 300,
      status,
      json: () => Promise.resolve(body),
    });
    if (url === '/api/tracks') return json(200, tracks);
    if (url === '/api/playlist' && !init) return json(200, playlist);
    if (url === '/api/playlist/tracks' && init?.method === 'POST') {
      const { trackId } = JSON.parse(init.body as string);
      const status = typeof postStatus === 'function' ? postStatus(trackId) : postStatus;
      const track = tracks.find((t) => t.id === trackId)!;
      if (status === 201) playlist = [...playlist, track];
      return json(status, status === 201 ? track : { error: 'x' });
    }
    return json(404, {});
  });
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

describe('App', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('shows the catalog tracks and the empty-state text', async () => {
    stubApi();
    render(<App />);
    expect(await screen.findByRole('button', { name: 'Add Midnight Static to playlist' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Add Green Pipeline to playlist' })).toBeInTheDocument();
    expect(
      screen.getByText('Your playlist is empty. Add a track to get started.'),
    ).toBeInTheDocument();
  });

  it('adds a track and hides the empty state', async () => {
    stubApi();
    render(<App />);
    fireEvent.click(await screen.findByRole('button', { name: 'Add Midnight Static to playlist' }));
    const list = await screen.findByRole('list', { name: 'Playlist tracks' });
    expect(within(list).getByText(/Midnight Static/)).toBeInTheDocument();
    expect(screen.queryByText(/Your playlist is empty/)).not.toBeInTheDocument();
  });

  it('shows the duplicate message as an alert and re-announces a repeat', async () => {
    stubApi((id) => (id === 1 ? 409 : 201));
    render(<App />);
    const button = await screen.findByRole('button', { name: 'Add Midnight Static to playlist' });
    fireEvent.click(button);
    expect(await screen.findByRole('alert')).toHaveTextContent('This track is already in your playlist.');
    fireEvent.click(button);
    expect(await screen.findByRole('alert')).toHaveTextContent('This track is already in your playlist.');
    expect(screen.getByText(/Your playlist is empty/)).toBeInTheDocument();
  });

  it('shows an alert when the API cannot be reached', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500, json: () => Promise.resolve({}) }));
    render(<App />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Could not reach the API');
  });
});
