using UnityEngine;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// Cytokine sensing (GAME_DESIGN.md sections 2a/7/9, rung 2 of the
    /// search ladder).
    ///
    /// **Sprint 12 (Director, 2026-08-29): sensing is ON by default and is
    /// no longer a player-facing choice** — every unit senses; a *buyable
    /// upgrade* (`ShopItem.CytokineSensingUpgrade` → `Chemotaxis.SensingUpgradeLevel`)
    /// sharpens it. The `C` key stays as a debug OFF-toggle so a build can
    /// still show the rung-1-vs-rung-2 contrast for comparison, but it
    /// starts ON. Read by SearchUnit via `Enabled`; the HUD shows state.
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
        public static bool Enabled { get; private set; } = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Enabled = !Enabled;
            }
        }
    }
}
