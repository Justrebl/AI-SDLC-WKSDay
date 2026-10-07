# Later slice decision: mood filter on the track catalog

**Status:** Proposed for Level 5 Copilot cloud agent delegation. Not implemented. Not validated.

This slice is separate from the shared Level 3 playlist scope, which stays unchanged. See [playlist design decisions](playlist-design-decisions.md).

## Problem

Listeners browsing the catalog have no way to narrow the list to the kind of music that fits their current mood. The Design Thinking exploration raised this as a hypothesis. No listener research has confirmed it.

## User value

A listener can narrow the catalog to a mood while browsing, so choosing a track takes less scanning.

## Chosen scope

- Add a hand-entered `mood` tag to each track in the seed data. The vocabulary is `upbeat` and `calm`, with one tag per track.
- `GET /api/tracks` accepts an optional `mood` query parameter.
- The catalog shows labelled mood buttons, including "All", that narrow the displayed list.
- State stays in memory. The existing `src/api` and `src/front` setup is kept.

## Acceptance criteria

- Each track in the API response has a camelCase `mood` field.
- `GET /api/tracks?mood=upbeat` returns only tracks tagged `upbeat`.
- `GET /api/tracks` with no `mood` returns all tracks.
- An unknown `mood` value returns an empty list with HTTP 200.
- Mood buttons are labelled, include "All", and expose the selected state with `aria-pressed`.
- Selecting a mood updates the visible catalog list.
- When no track matches, the page shows "No tracks match this mood." with `role="status"`.
- xUnit tests in `tests/api` and Vitest with Testing Library tests in `src/front` cover the behaviour and pass.

## Exclusions

- Playback, queues, and dimming or skipping upcoming tracks.
- Automatic tagging, voice controls, and smartwatch interaction.
- Authentication, a database, persistence, and external services.
- Any change to the Level 3 playlist endpoints, states, or behaviour.

## Assumptions (unvalidated)

- Mood is a meaningful way for listeners to narrow choices.
- One tag per track and two moods are enough for a first slice.
- Hand-entered tags are acceptable placeholders, not a real classification.

## Dependencies

- The Level 3 catalog list and `GET /api/tracks` must exist, because the filter extends them.
- The filter must not alter playlist behaviour.

## Rationale

- It is a small, observable change that fits the existing ASP.NET Core API, React front end and in-memory state.
- It keeps the mood idea from the exploration alive without the larger playback and queue behaviour.
- Alternatives considered were an up-next preview, which overlaps the Level 3 playlist, and a thumbs feedback counter, which says little without a mood feature.

## Evidence limits

The idea came from a short sampling exercise with no real listeners, observation, or tests. Treat it as a hypothesis to check, not a research finding.
