# Interface Contract (Engine ↔ UI)

Status: not yet defined — there's no gameplay code for UI to depend on yet.
The first version of this file should be drafted during Sprint 0 or
Sprint 1, jointly, once Code has a basic data shape for towers/enemies and
UI needs to know what it can read.

When this fills in, it should cover things like: what data a tower/enemy
object exposes (stats, state), event names UI can subscribe to (e.g. "tower
placed," "enemy killed," "wave started"), and anything else either side
depends on that would break the other side silently if changed without
warning. Changing something here is a cross-team event — flag it to both
sessions before changing it, not after.
