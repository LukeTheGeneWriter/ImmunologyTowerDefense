using UnityEngine;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// Sprint 1's debug toggle for cytokine sensing (GAME_DESIGN.md
    /// sections 2a/7/9, rung 2 of the search ladder). Runtime-toggleable
    /// with the C key so it works in a standalone build, not just in the
    /// Editor -- an Inspector checkbox alone wouldn't be reachable once
    /// packaged, and the Director watches a build, not the Editor. Read by
    /// SearchUnit via the static Enabled flag; the HUD (see
    /// Rendering/HudOverlay.cs) reads it too, to show current state.
    ///
    /// No UnityEngine.UI dependency here on purpose -- the com.unity.ugui
    /// package isn't in this project's manifest, and adding a package
    /// needs network access and is normally an Editor-GUI/Director step
    /// (see docs/ENGINE_STATUS.md's Steam integration note for the same
    /// constraint). IMGUI (OnGUI) needs no extra package and is a natural
    /// fit for a debug overlay anyway.
    /// </summary>
    public class CytokineToggle : MonoBehaviour
    {
        public static bool Enabled { get; private set; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Enabled = !Enabled;
            }
        }
    }
}
