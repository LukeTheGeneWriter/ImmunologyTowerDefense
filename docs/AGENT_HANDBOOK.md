# Agent Handbook

Curated and evergreen, owned by HR Claude — distilled from `TEAM_RETRO.md`.
See `WORKFLOW.md` Section 6.2 for the intended format. Every brief to a
fresh Code or UI session should point at this file alongside their own
status doc.

## Tips & tricks

(None yet.)

## Known points of difficulty

- **Device-bridge git commands strand lock files.** Any `git` command run through the Claude desktop device bridge (`device_bash`) tends to leave a stray `.git/index.lock`, `.git/HEAD.lock`, and/or `.git/objects/maintenance.lock` behind, because the bridge can rename files but can't unlink them, and git sometimes needs a plain unlink to release its own lock. Left in place, this makes the Director's own git client fail with "another git process seems to be running." Standing fix: after any git command sequence run through the bridge, do one final pass moving any `.git/*.lock` (and `.git/objects/maintenance.lock`) into `_to_delete/`, as the *very last* step, with no git command after it (since even `git status` can strand a fresh one).

## Contact protocol

Default, per `WORKFLOW.md` Section 1: Code and UI route cross-cutting
concerns through the Producer rather than messaging each other directly.
No exceptions have been established yet — there isn't enough history to
justify one.
