# Team Retro Log

Raw, dated notes from Code, UI, or the Producer — anything that was harder
than it should have been, or easier because of a tip left behind. A few
lines per sprint. Never overwritten, only appended to. See `WORKFLOW.md`
Section 6.1 for the intended format.

(Empty — Sprint 0 hasn't run yet.)

### Sprint 0 — Producer
- Discovered the Claude desktop device bridge's shell (`device_bash`) runs in
  an isolated Linux sandbox, not the real Windows OS — it can read/write
  files in a connected folder (git add/commit worked fine there) but cannot
  execute Windows binaries like Unity.exe. Tip for next Code session: any
  Unity CLI/batchmode step needs to be run by the Director directly, or by
  a local Claude Code session running natively on the Director's machine —
  not assumed to be scriptable through the device bridge.
