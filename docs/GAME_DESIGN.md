# Immunology Tower Defense — Game Design

Status: seed draft, written before Sprint 0. Everything below is either a
locked decision or explicitly marked TBD. Producer keeps this current as
Director decisions land; Code and UI both build against this file.

## Locked decisions

- **Engine:** Unity (decided 2026-08-18 — see rationale in `ENGINE_STATUS.md`).
- **Platforms:** Steam (primary release) + a web-based build (public-facing
  funnel/demo, in the spirit of Bloons TD's web presence). Mobile is not
  ruled out later, but is not in scope for early sprints.
- **Genre:** Tower defense, immunology-themed — towers as immune cells,
  enemies as pathogens/antigens. Exact roster is TBD.

## TBD — needs a Direction conversation before Sprint 1

- Core loop specifics: wave structure, economy, win/loss conditions.
- Tower roster (which immune cells, what they do, how they're unlocked).
- Enemy roster (pathogen types, boss design, scaling curve).
- Art direction / tone (clinical and precise vs. stylized and playful).
- Meta-progression, if any (persistent unlocks across runs).

## Performance requirement (non-negotiable, not just a nice-to-have)

Research into comparable games (Bloons TD 6 specifically) shows late-game
slowdown is driven by raw entity count — enemies, projectiles, tower
effects, and ability animations all stacking up at once — not by engine
choice. This game is expected to reach similarly dense late rounds, so:

- Enemies, projectiles, and effects must be object-pooled from the first
  implementation, not retrofitted later.
- There should be an explicit, tunable cap on simultaneous cosmetic effects
  (particles, hit-flashes, etc.) that degrades gracefully under load rather
  than accumulating unbounded.

This requirement belongs in every relevant sprint brief until the core
combat loop is fully pooled — see `ENGINE_STATUS.md`.
