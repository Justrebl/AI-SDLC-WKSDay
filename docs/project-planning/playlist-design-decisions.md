# Playlist design decisions

This page records the decisions behind the shared Music Catalog playlist slice. It is a curated reference, not a research report.

## Evidence status

- The playlist feature is a **facilitator-supplied workshop scope**. It was not derived from, or validated by, the Design Thinking exploration.
- The exploration was a short learning exercise that sampled the nine methods. It involved no real listeners, observation, interviews or tests, so its outputs are hypotheses.
- No method was completed in full, and the concept is not validated.

## Shared playlist delivery contract

Source: [Debrief and hand off to the shared implementation slice](../afternoon-2/workshop.md#debrief-and-hand-off-to-the-shared-implementation-slice).

| Area | Decision |
| --- | --- |
| Capability | Browse tracks and add them to one in-memory playlist |
| API | `GET /api/tracks`, `GET /api/playlist`, `POST /api/playlist/tracks` with a JSON body containing `trackId` |
| Front end | Visible catalog and playlist; empty state text "Your playlist is empty. Add a track to get started." |
| Errors | Unknown track id returns HTTP 404; duplicate add returns HTTP 409 |
| Duplicate feedback | Must be visible and accessible; the UX approach is **open** until the RPI plan gate |
| Accessibility | Labelled controls and perceivable status feedback |
| Out of scope | Users, authentication, persistence, reorder, remove, search, playlist creation |

## Design Thinking exploration (hypotheses only)

- **Starting question:** how might we help someone choose music for a listening moment?
- **Explored audience (assumed):** tech-savvy hi-fi enthusiasts listening at home.
- **Explored direction:** a mood filter that leaves the current track playing and skips non-matching upcoming tracks, with mood tags entered by hand.
- **Status:** not part of the playlist slice, and not validated. It is kept as possible future work.

## Assumptions still to check

- Listeners choose the next track while listening.
- Seeing what is coming next has value for them.
- Mood is a useful way to narrow choices, and hand-entered tags are an acceptable placeholder.

## Open decisions

- How the interface reports duplicate adds (decided at the RPI plan gate).
- Whether any mood-based follow-up is worth pursuing after real listener research.

## Planned work, not done

- Observe real listeners.
- Test a low-fidelity prototype without leading participants.
- Define success signals before any experiment.
