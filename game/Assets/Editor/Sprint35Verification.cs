using ImmunologyTD.Adaptive;
using ImmunologyTD.Bootstrap;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;
using ImmunologyTD.Rounds;
using ImmunologyTD.UI;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 35 — **the harness can see the screen.**
///
/// ## Why this file is different from the other thirty
///
/// Every harness before it asserts that a value is *returned*. Sprint 33's
/// buy-phase caption had 41 such checks, all green, and the Director saw
/// nothing in play. That is the eighth instance of this project's standing
/// pattern — a value produced on one side of a boundary and never consumed
/// as that value on the other — and the first on the UI side, where the
/// consumer is a pixel.
///
/// So the first part of this harness boots the real game, drives a real
/// `RoundController` through its phases, and reads the **live visual
/// tree**: what each element says, and whether anything hides it.
///
/// ## What it proved, and what it could not
///
/// Run against the shipped build before anything was changed, it found the
/// caption **correct** — attached, displayed, unique, renderable, in every
/// phase. The defect was never a wire. The round's identity was 11 px of
/// dim grey in a corner, under two 30 px numbers, with four facts run
/// together; in a browser window narrower than the 1920 px reference the
/// panel scales that down further. So the fix was a surface
/// (`RoundBannerView`), not a wire, and this harness pins the surface.
///
/// **The honest limit:** batchmode runs no layout pass, so `worldBound` is
/// all zeroes and nothing here can prove a line is legible, unclipped or
/// on top. `UiProbe.Dump` and `UiTreeDump` exist for that gap, and an eye
/// on a real build is still required. A check that pretended otherwise
/// would be the same mistake one level up.
/// </summary>
public static class Sprint35Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 35 -- the screen says it")]
    public static void RunAll()
    {
        checks = 0; failures = 0;
        Random.InitState(3501);

        Debug.Log("[Sprint35Verification] === The screen says it ===");

        RunTheProbeCanFail();
        RunTheRoundMessageReachesTheTree();
        RunTheDebutCardTeaches();
        RunEveryWarningIsSayable();
        RunPathogensAreDrawnAsThemselves();
        RunTheTwoFormPathIsReachable();
        RunTheBarcodeHoldsTheWholePlannedRoster();
        RunTheRibbonShowsWhatYouHaveMet();
        RunTheRibbonScrollsAndClamps();
        RunRungsAreLabelledByCapability();
        RunVibrioRunsStraight();

        if (failures > 0)
            Debug.LogError($"[Sprint35Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint35Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint35Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint35Verification] FAIL -- {label}"); }
    }

    // ================================================================
    // 0. The probe itself. A check that can only ever answer "displayed"
    //    is not a check, and this harness is built entirely on the probe.
    // ================================================================
    private static void RunTheProbeCanFail()
    {
        Debug.Log("[Sprint35Verification] --- The probe can fail ---");

        UiProbe.Clear();
        Check("with no UI registered, nothing is found", UiProbe.Find(UiNames.HudCaption) == null);
        Check("and nothing is displayed", !UiProbe.IsDisplayed(UiNames.HudCaption));

        // A tree built by hand, so the probe's two real failure modes can
        // be provoked deliberately rather than waited for.
        var root = new UnityEngine.UIElements.VisualElement { name = "probe-root" };
        var hidden = new UnityEngine.UIElements.VisualElement { name = "hidden-parent" };
        var child = new UnityEngine.UIElements.Label("I am here") { name = "buried-child" };
        hidden.Add(child);
        root.Add(hidden);
        var orphan = new UnityEngine.UIElements.Label("nobody's child") { name = "orphan" };
        UiProbe.Register(root);

        Check("a present element is displayed", UiProbe.IsDisplayed("buried-child"));
        UiTheme_Show(hidden, false);
        Check("hiding the PARENT hides the child", !UiProbe.IsDisplayed("buried-child"));
        Check($"and the reason names the parent (\"{UiProbe.HiddenBecause("buried-child")}\")",
              UiProbe.HiddenBecause("buried-child")?.Contains("hidden-parent") == true);
        UiTheme_Show(hidden, true);
        Check("showing it again restores the child", UiProbe.IsDisplayed("buried-child"));

        Check("a missing name is reported as missing",
              UiProbe.HiddenBecause("no-such-element")?.Contains("no element named") == true);
        Check("text is read back from the live element", UiProbe.TextOf("buried-child") == "I am here");
        Check("an unparented element is not found from the root", UiProbe.Find("orphan") == null);
        // NOT the gamma-delta example: Sprint 26's ImmunoSans merged the
        // Greek block in, so Greek renders now and a guard built on it
        // could never fail. A CJK ideograph is outside every range the
        // shipped faces carry -- and asserting that Greek DOES render
        // keeps the other half of Sprint 26's fix pinned too.
        Check("the font guard still rejects what the font cannot draw",
              !UiProbe.IsRenderable("一") && UiProbe.IsRenderable("IFN-γ"));

        _ = orphan;
        UiProbe.Clear();
    }

    /// <summary>`UiTheme` is internal, so the Editor assembly sets display
    /// the same way it does -- directly. Keeping this one line local is
    /// cheaper than widening the theme's visibility for a test.</summary>
    private static void UiTheme_Show(UnityEngine.UIElements.VisualElement v, bool visible) =>
        v.style.display = visible
            ? UnityEngine.UIElements.DisplayStyle.Flex
            : UnityEngine.UIElements.DisplayStyle.None;

    // ================================================================
    // 1-3. The live tree, through a real boot and real phase changes.
    // ================================================================

    /// <summary>
    /// One whole booted game, and the trap that comes with booting more
    /// than one of them in a single Editor process.
    ///
    /// **`GameBootstrap` creates many ROOT GameObjects** -- UiRoot, the
    /// spawner, the flash pool, the organs -- not children of the object
    /// the harness made. Destroying only that object therefore leaves an
    /// entire parallel game in the scene, and the next
    /// `FindFirstObjectByType&lt;UiController&gt;()` returns the **stale**
    /// one while `UiProbe` answers for the newest tree. The first run of
    /// this harness failed exactly that way: the round-1 banner checks
    /// passed (one boot) and every check in the second and later boots
    /// read a tree nobody was refreshing.
    ///
    /// So this records the scene's roots before booting and takes down
    /// everything that appeared, which is the only safe meaning of
    /// "dispose" for a bootstrap that owns the scene.
    /// </summary>
    private sealed class Booted : System.IDisposable
    {
        public readonly GameObject Go;
        public readonly UiController Ui;
        public readonly RoundController Rounds;
        public readonly PathogenSpawner Spawner;

        private readonly System.Collections.Generic.HashSet<GameObject> before =
            new System.Collections.Generic.HashSet<GameObject>();

        public Booted()
        {
            UiProbe.Clear();
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.transform.parent == null) before.Add(go);

            Go = new GameObject("S35Bootstrap");
            Go.AddComponent<BoardConfig>();
            var boot = Go.AddComponent<GameBootstrap>();
            typeof(GameBootstrap)
                .GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(boot, null);
            Ui = Object.FindFirstObjectByType<UiController>();
            Rounds = Object.FindFirstObjectByType<RoundController>();
            Spawner = Object.FindFirstObjectByType<PathogenSpawner>();

            // If a previous boot leaked, the components found above may not
            // be this one's. Say so loudly rather than producing checks
            // that quietly describe the wrong game.
            if (Ui != null && UiProbe.Root == null)
                Debug.LogError("[Sprint35Verification] FAIL -- the UiController found is not the one just booted "
                               + "(a previous boot leaked into the scene).");
            Ui?.RefreshViews();
        }

        /// <summary>Starts the round and runs it to its end through the
        /// real spawner, which is what returns the controller to a buy
        /// phase. A phase forced by reflection would prove nothing about
        /// what the player meets.</summary>
        public void PlayOneRound()
        {
            Rounds.StartRound();
            Ui.RefreshViews();
            float t = 0f;
            for (int i = 0; i < 40000 && Rounds.Phase == RoundPhase.Active; i++)
            {
                t += 0.05f;
                Spawner.Tick(0.05f, t);
                Rounds.Tick(0.05f);
            }
            Ui.RefreshViews();
        }

        public void Dispose()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go != null && go.transform.parent == null && !before.Contains(go))
                    Object.DestroyImmediate(go);
            UiProbe.Clear();
        }
    }

    private static void RunTheRoundMessageReachesTheTree()
    {
        Debug.Log("[Sprint35Verification] --- The round message reaches the tree ---");
        SpeciesRoster.ResetToDefaults();

        using (var b = new Booted())
        {
            Check("the game booted with a UI, a round controller and a spawner",
                  b.Ui != null && b.Rounds != null && b.Spawner != null);
            Check("the panel root is registered with the probe", UiProbe.Root != null);

            // -- the opening buy phase --
            Check("the HUD caption exists exactly once", UiProbe.Count(UiNames.HudCaption) == 1);
            string cap = UiProbe.TextOf(UiNames.HudCaption);
            Check($"it says something (\"{cap}\")", !string.IsNullOrEmpty(cap));
            Check($"the player would see it ({UiProbe.HiddenBecause(UiNames.HudCaption) ?? "displayed"})",
                  UiProbe.IsDisplayed(UiNames.HudCaption));
            Check("and the shipped font can draw every character of it", UiProbe.IsRenderable(cap));

            // -- the banner: the surface the message actually lives on --
            Check($"the round banner is up in the buy phase ({UiProbe.HiddenBecause(UiNames.RoundBanner) ?? "displayed"})",
                  UiProbe.IsDisplayed(UiNames.RoundBanner));
            string vehicle = UiProbe.TextOf(UiNames.RoundBannerVehicle);
            string tagline = RoundScript.ForRound(1).Tagline;
            Check($"and it names round 1's vehicle: \"{vehicle}\"", vehicle == tagline);
            Check("the vehicle line is 22 px, not 11 -- the whole point of the fix",
                  FontSizeOf(UiNames.RoundBannerVehicle) >= 20f);

            // The caption must no longer duplicate what the banner says.
            Check("the HUD caption no longer repeats the tagline",
                  !string.IsNullOrEmpty(cap) && !cap.Contains(tagline));

            // -- during the round --
            b.Rounds.StartRound();
            b.Ui.RefreshViews();
            Check("starting the round takes the banner down",
                  !UiProbe.IsDisplayed(UiNames.RoundBanner));
            string active = UiProbe.TextOf(UiNames.HudCaption);
            Check($"the caption switches to the active rule (\"{active}\")",
                  !string.IsNullOrEmpty(active) && active != cap && UiProbe.IsRenderable(active));

            // -- and back to a buy phase, the honest way --
            float t = 0f;
            for (int i = 0; i < 40000 && b.Rounds.Phase == RoundPhase.Active; i++)
            {
                t += 0.05f;
                b.Spawner.Tick(0.05f, t);
                b.Rounds.Tick(0.05f);
            }
            b.Ui.RefreshViews();
            Check($"round 1 really resolved (phase={b.Rounds.Phase}, cleared={b.Rounds.RoundsCleared})",
                  b.Rounds.Phase != RoundPhase.Active);

            if (b.Rounds.Phase == RoundPhase.Building)
            {
                Check("the banner comes back for the next round",
                      UiProbe.IsDisplayed(UiNames.RoundBanner));
                Check($"and it now names round 2's vehicle: \"{UiProbe.TextOf(UiNames.RoundBannerVehicle)}\"",
                      UiProbe.TextOf(UiNames.RoundBannerVehicle) == RoundScript.ForRound(2).Tagline);
            }
            else
            {
                // A run that is lost in round 1 is a legitimate outcome of
                // a harness with no towers placed; say so rather than
                // asserting on a phase that never came.
                Debug.Log($"[Sprint35Verification] NOTE -- round 1 ended in {b.Rounds.Phase}; " +
                          "the round-2 banner checks do not apply to a lost run.");
            }
        }
    }

    private static float FontSizeOf(string name)
    {
        var e = UiProbe.Find(name);
        return e == null ? 0f : e.style.fontSize.value.value;
    }

    private static void RunTheDebutCardTeaches()
    {
        Debug.Log("[Sprint35Verification] --- The debut card teaches ---");
        SpeciesRoster.ResetToDefaults();

        // Round 6 is Vibrio cholerae's debut in the shipped script, so the
        // card is checked where the script actually puts a first encounter
        // rather than at a round picked to make the test pass.
        int debutRound = -1;
        for (int r = 1; r <= RoundScript.ScriptedRounds; r++)
            if (System.Array.IndexOf(RoundScript.DebutsInRound(r), PathogenSpecies.VibrioCholerae) >= 0)
            { debutRound = r; break; }
        Check($"the script still debuts Vibrio cholerae somewhere (round {debutRound})", debutRound > 0);

        var debuts = RoundScript.DebutsInRound(debutRound);
        Check("and it is the only organism debuting that round", debuts.Length == 1);

        string warning = SpeciesRoster.WarningOf(PathogenSpecies.VibrioCholerae);
        Check($"it has a warning: \"{warning}\"", !string.IsNullOrEmpty(warning));

        // §4.3: the warning is a NEGATIVE -- it names what will not work.
        // Asserted as a property of the text rather than a spot check, so a
        // later rewrite that turns it into a hint ("use IgA") fails here.
        int negatives = 0;
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
        {
            string w = SpeciesRoster.WarningOf(sp).ToLowerInvariant();
            if (w.Contains("not ") || w.Contains("cannot") || w.Contains("will not")
                || w.Contains("no ") || w.Contains("nothing")) negatives++;
        }
        Check($"every warning is phrased as a negative ({negatives}/{SpeciesRoster.Count})",
              negatives == SpeciesRoster.Count);

        using (var b = new Booted())
        {
            // Walk the controller to the buy phase BEFORE the debut round
            // by playing rounds, then read what the card says.
            int guard = 0;
            while (b.Rounds.RoundNumber + 1 < debutRound && guard++ < debutRound + 2
                   && b.Rounds.Phase == RoundPhase.Building)
                b.PlayOneRound();

            if (b.Rounds.Phase == RoundPhase.Building && b.Rounds.RoundNumber + 1 == debutRound)
            {
                Check($"the debut card is up in round {debutRound}'s buy phase",
                      UiProbe.IsDisplayed(UiNames.RoundBanner));
                Check($"it names the organism: \"{UiProbe.TextOf(UiNames.RoundBannerName)}\"",
                      UiProbe.TextOf(UiNames.RoundBannerName)
                          == SpeciesRoster.NameOf(PathogenSpecies.VibrioCholerae));
                Check("it carries the warning line",
                      UiProbe.TextOf(UiNames.RoundBannerWarning) == warning);
                Check("the warning is displayed, not merely written",
                      UiProbe.IsDisplayed(UiNames.RoundBannerWarning));
                Check("and the portrait draws the organism's own sprite",
                      UiProbe.HasBackgroundSprite(UiNames.RoundBannerPortrait,
                                                  SpriteShapes.ForSpecies(PathogenSpecies.VibrioCholerae)));
            }
            else
            {
                Debug.Log($"[Sprint35Verification] NOTE -- could not reach round {debutRound}'s buy phase " +
                          $"headlessly (phase={b.Rounds.Phase}, round={b.Rounds.RoundNumber}); " +
                          "the in-tree debut-card checks were skipped. The data-side checks above still hold.");
            }
        }
    }

    private static void RunEveryWarningIsSayable()
    {
        Debug.Log("[Sprint35Verification] --- Every warning is sayable ---");
        SpeciesRoster.ResetToDefaults();

        int missing = 0, unrenderable = 0;
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
        {
            string w = SpeciesRoster.WarningOf(sp);
            if (string.IsNullOrEmpty(w)) { missing++; continue; }
            if (!UiProbe.IsRenderable(w)) { unrenderable++; Debug.LogError($"  {sp}: \"{w}\""); }
        }
        Check($"every species has a warning ({SpeciesRoster.Count - missing}/{SpeciesRoster.Count})", missing == 0);
        // The gamma-delta lesson: UI Toolkit draws nothing for a missing
        // glyph -- no warning, no error, no tofu box.
        Check($"and the shipped font can draw all of them ({unrenderable} cannot)", unrenderable == 0);
    }

    // ================================================================
    // 4-5. Sprites: per species, and two forms.
    // ================================================================
    private static void RunPathogensAreDrawnAsThemselves()
    {
        Debug.Log("[Sprint35Verification] --- Drawn as themselves ---");
        SpeciesRoster.ResetToDefaults();

        var named = new[]
        {
            PathogenSpecies.Rotavirus, PathogenSpecies.VibrioCholerae, PathogenSpecies.AttachingEffacingEcoli,
        };

        // The actual Sprint 24 defect: a named organism rendering as its
        // class generic. Asserted per species against the generic it used
        // to be drawn as.
        foreach (var sp in named)
        {
            var generic = SpeciesRoster.GenericFor(SpeciesRoster.ClassOf(sp));
            Check($"{sp} is not drawn as {generic}",
                  SpriteShapes.ForSpecies(sp) != SpriteShapes.ForSpecies(generic));
        }

        // And distinct from EACH OTHER -- two species sharing one new
        // sprite would pass the check above and still be unreadable.
        int collisions = 0;
        var all = (PathogenSpecies[])System.Enum.GetValues(typeof(PathogenSpecies));
        for (int i = 0; i < all.Length; i++)
            for (int j = i + 1; j < all.Length; j++)
                if (System.Array.IndexOf(named, all[i]) >= 0 && SpriteShapes.ForSpecies(all[i]) == SpriteShapes.ForSpecies(all[j]))
                    collisions++;
        Check($"no named organism shares a silhouette ({collisions} collisions)", collisions == 0);

        // Hue stays inside the family (spec §2.2): a named virus is some
        // violet, a named bacterium some green -- never a new hue, because
        // every other hue on this board already means something.
        foreach (var sp in named)
        {
            var c = Palette.ForSpecies(sp);
            bool virus = SpeciesRoster.ClassOf(sp) == PathogenClass.IntracellularVirus;
            bool inFamily = virus ? (c.b > c.g && c.r > c.g) : (c.g > c.r && c.g > c.b);
            Check($"{sp}'s colour stays in its family ({(virus ? "violet" : "green")})", inFamily);
        }

        // The knowledge HUD's live bug: SpeciesIcon switched on
        // case 0 / case 1 / default, and rotavirus is species 3.
        using (var b = new Booted())
        {
            string card = UiNames.KnowledgeSpecies((int)PathogenSpecies.Rotavirus);
            Check($"the knowledge HUD has a card for rotavirus ({card})", UiProbe.Exists(card));
            Check("and it draws the rotavirus sprite, not a bacterium",
                  UiProbe.HasBackgroundSprite(card, SpriteShapes.Rotavirus));
            Check("the generic bacterium's card draws the bacterium",
                  UiProbe.HasBackgroundSprite(UiNames.KnowledgeSpecies((int)PathogenSpecies.GenericLargeBacterium),
                                              SpriteShapes.LargeBacterium));
        }
    }

    private static void RunTheTwoFormPathIsReachable()
    {
        Debug.Log("[Sprint35Verification] --- Two forms, on the board ---");
        SpeciesRoster.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();

        Check("A/E E. coli has a second form",
              SpriteShapes.HasAlternateForm(PathogenSpecies.AttachingEffacingEcoli));
        int withAlternate = 0;
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
            if (SpriteShapes.HasAlternateForm(sp)) withAlternate++;
        Check($"and it is the only one, so the path has exactly one shipped user ({withAlternate})",
              withAlternate == 1);

        // Measured through the real GutWall, not a reimplementation of it.
        var boardGo = new GameObject("S35_board");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var gut = new GutWall(board, grid, tally);

        var go = new GameObject("S35_aeec");
        go.AddComponent<SpriteRenderer>();
        var agent = go.AddComponent<PathogenAgent>();
        agent.Initialize(board, grid, gut, tally, null, null,
                         speciesOverride: PathogenSpecies.AttachingEffacingEcoli);

        var sr = go.GetComponent<SpriteRenderer>();
        Check($"free in the lumen it is drawn in its primary form ({agent.Form})",
              agent.Form == PathogenForm.Primary
              && sr.sprite == SpriteShapes.ForSpecies(PathogenSpecies.AttachingEffacingEcoli, PathogenForm.Primary));

        bool stuck = agent.AdhereToInterface(board.LumenEntryCrossIndex);
        Check("it adheres to the wall", stuck);
        Check($"and raises its pedestal there ({agent.Form})",
              agent.Form == PathogenForm.Alternate
              && sr.sprite == SpriteShapes.AttachingEffacingEcoliAnchored);

        agent.ReturnToFlow(board.LumenEntryCrossIndex, 0f);
        Check($"being sloughed costs it the anchor ({agent.Form})",
              agent.Form == PathogenForm.Primary
              && sr.sprite == SpriteShapes.ForSpecies(PathogenSpecies.AttachingEffacingEcoli, PathogenForm.Primary));

        // A species with one shape must not be churned through a form
        // change it does not have.
        var go2 = new GameObject("S35_vibrio");
        go2.AddComponent<SpriteRenderer>();
        var vibrio = go2.AddComponent<PathogenAgent>();
        vibrio.Initialize(board, grid, gut, tally, null, null, speciesOverride: PathogenSpecies.VibrioCholerae);
        vibrio.AdhereToInterface(board.LumenEntryCrossIndex + 1);
        Check($"a one-form organism stays in its primary form on the wall ({vibrio.Form})",
              vibrio.Form == PathogenForm.Primary
              && go2.GetComponent<SpriteRenderer>().sprite == SpriteShapes.Vibrio);

        Object.DestroyImmediate(go2);
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(boardGo);
        RoundClock.Reset();
    }

    // ================================================================
    // 6. The barcode, for the roster that is coming -- not just the one
    //    that is here. This is the gate the 8-bit mistake taught.
    // ================================================================
    private static void RunTheBarcodeHoldsTheWholePlannedRoster()
    {
        Debug.Log("[Sprint35Verification] --- The barcode holds 16 ---");
        AdaptiveTuning.ResetToDefaults();
        SpeciesRoster.ResetToDefaults();

        Check($"the barcode is 12 bits ({AdaptiveTuning.BarcodeBits})", AdaptiveTuning.BarcodeBits == 12);

        // The full planned set: six live tags plus the ten held for the
        // specified-but-unbuilt organisms.
        var tags = new System.Collections.Generic.List<(string name, int tag)>();
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
            tags.Add((sp.ToString(), SpeciesRoster.Get(sp).Antigen));
        foreach (var r in SpeciesRoster.ReservedAntigens) tags.Add((r.name, r.antigen));

        Check($"the planned roster is 16 organisms ({tags.Count})", tags.Count == 16);

        int limit = 1 << AdaptiveTuning.BarcodeBits;
        int tooWide = 0;
        foreach (var t in tags) if (t.tag < 0 || t.tag >= limit) tooWide++;
        Check($"every tag fits in {AdaptiveTuning.BarcodeBits} bits ({tooWide} do not)", tooWide == 0);

        int worst = int.MaxValue;
        string worstPair = "";
        int pairs = 0;
        for (int i = 0; i < tags.Count; i++)
            for (int j = i + 1; j < tags.Count; j++)
            {
                pairs++;
                int d = Antigen.HammingDistance(tags[i].tag, tags[j].tag);
                if (d < worst) { worst = d; worstPair = $"{tags[i].name}/{tags[j].name}"; }
            }
        int required = 2 * AdaptiveTuning.MatchMaxHammingDistance + 1;
        Check($"all {pairs} pairs are >= 2r+1 = {required} apart (worst {worst}, {worstPair})", worst >= required);
        Check($"and the set really sits 6 apart, as the spec measured (worst {worst})", worst == 6);

        // The property itself, over the whole code space: no helper-T tag
        // can teach two of the SIXTEEN. This is what 10 bits could not do
        // and what the sprint widened the barcode for.
        int ambiguous = 0;
        for (int tag = 0; tag < limit; tag++)
        {
            int n = 0;
            for (int i = 0; i < tags.Count; i++)
                if (Antigen.HammingDistance(tag, tags[i].tag) <= AdaptiveTuning.MatchMaxHammingDistance) n++;
            if (n > 1) ambiguous++;
        }
        Check($"no tag in the whole 12-bit space teaches two of the sixteen ({ambiguous}/{limit})",
              ambiguous == 0);

        // And the correction to the comment that claimed otherwise: this
        // set is NOT safe at r=3, which is now a stated consequence of
        // buying avidity rather than an accident nobody had measured.
        int ambiguousAtThree = 0;
        for (int tag = 0; tag < limit; tag++)
        {
            int n = 0;
            for (int i = 0; i < tags.Count; i++)
                if (Antigen.HammingDistance(tag, tags[i].tag) <= 3) n++;
            if (n > 1) ambiguousAtThree++;
        }
        Check($"at r=3 it cross-teaches deliberately, as documented ({ambiguousAtThree} tags do)",
              ambiguousAtThree > 0);
    }

    // ================================================================
    // 7-8. The ribbon: the Director's design, asserted.
    // ================================================================
    private static void RunTheRibbonShowsWhatYouHaveMet()
    {
        Debug.Log("[Sprint35Verification] --- Five most recently seen ---");
        SpeciesRoster.ResetToDefaults();
        SpeciesSightings.ResetForNewRun();

        var order = new PathogenSpecies[SpeciesRoster.Count];
        int n = SpeciesSightings.OrderForDisplay(order);
        Check($"with nothing seen, the order is the whole roster ({n})", n == SpeciesRoster.Count);
        Check("and nothing is marked as met", SpeciesSightings.DistinctSeen() == 0);

        // Meet three organisms in a known order, oldest first.
        SpeciesSightings.Record(PathogenSpecies.GenericLargeBacterium);
        SpeciesSightings.Record(PathogenSpecies.Rotavirus);
        SpeciesSightings.Record(PathogenSpecies.VibrioCholerae);
        SpeciesSightings.OrderForDisplay(order);

        Check($"the most recently seen comes first ({order[0]})", order[0] == PathogenSpecies.VibrioCholerae);
        Check($"then the one before it ({order[1]})", order[1] == PathogenSpecies.Rotavirus);
        Check($"then the first one met ({order[2]})", order[2] == PathogenSpecies.GenericLargeBacterium);
        Check("three distinct organisms are recorded as met", SpeciesSightings.DistinctSeen() == 3);

        int unseenAfterSeen = 0;
        for (int i = 0; i < 3; i++) if (!SpeciesSightings.HasSeen(order[i])) unseenAfterSeen++;
        for (int i = 3; i < SpeciesRoster.Count; i++) if (SpeciesSightings.HasSeen(order[i])) unseenAfterSeen++;
        Check($"everything met sorts ahead of everything unmet ({unseenAfterSeen} out of place)",
              unseenAfterSeen == 0);

        // Seeing an old organism again moves it back to the front: that is
        // what "most recently seen" has to mean to be worth the Director's
        // five slots.
        SpeciesSightings.Record(PathogenSpecies.GenericLargeBacterium);
        SpeciesSightings.OrderForDisplay(order);
        Check($"meeting one again brings it to the front ({order[0]})",
              order[0] == PathogenSpecies.GenericLargeBacterium);

        // And the live view agrees with the pure function.
        using (var b = new Booted())
        {
            b.PlayOneRound();
            b.Ui.RefreshViews();
            var visible = b.Ui.KnowledgeVisibleSpecies();
            Check($"the ribbon's viewport holds five slots ({visible.Length})",
                  visible.Length == Mathf.Min(5, SpeciesRoster.Count));

            SpeciesSightings.OrderForDisplay(order);
            int mismatches = 0;
            for (int i = 0; i < visible.Length; i++) if (visible[i] != order[i]) mismatches++;
            Check($"and they are the first five of the display order ({mismatches} mismatches)", mismatches == 0);

            int met = SpeciesSightings.DistinctSeen();
            Check($"a round was actually fought, so something was met ({met})", met > 0);

            // Dimming means NEVER MET now, not "knowledge is zero".
            var unmet = PathogenSpecies.GenericVirus;
            foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
                if (!SpeciesSightings.HasSeen(sp)) { unmet = sp; break; }
            if (!SpeciesSightings.HasSeen(unmet))
            {
                float dim = UiProbe.OpacityOf(UiNames.KnowledgeSpecies((int)unmet));
                Check($"an organism never met is dimmed ({unmet}, opacity {dim})", dim < 0.9f);
            }
        }
    }

    private static void RunTheRibbonScrollsAndClamps()
    {
        Debug.Log("[Sprint35Verification] --- It scrolls, and it stops ---");

        using (var b = new Booted())
        {
            Check("the ribbon card and its viewport are in the tree",
                  UiProbe.Exists(UiNames.KnowledgeCard) && UiProbe.Exists(UiNames.KnowledgeViewport));

            // The gate: the viewport is bounded, so sixteen species cannot
            // grow a 3,000 px ribbon the way six already overflowed 420.
            var vp = UiProbe.Find(UiNames.KnowledgeViewport);
            float declared = vp.style.width.value.value;
            Check($"the viewport is five cards wide and stays there ({declared} px)",
                  Mathf.Abs(declared - 5 * 192f) < 0.5f);

            float range = b.Ui.KnowledgeScrollRange;
            Check($"with {SpeciesRoster.Count} species there is somewhere to scroll ({range} px)", range > 0f);
            Check($"and the range is exactly the overflow ({(SpeciesRoster.Count - 5) * 192f} px)",
                  Mathf.Abs(range - (SpeciesRoster.Count - 5) * 192f) < 0.5f);

            Check($"it starts at the front ({b.Ui.KnowledgeScrollOffset})", b.Ui.KnowledgeScrollOffset <= 0.5f);

            b.Ui.KnowledgeScrollBySlots(1);
            Check($"one notch moves it one card ({b.Ui.KnowledgeScrollOffset} px)",
                  Mathf.Abs(b.Ui.KnowledgeScrollOffset - 192f) < 0.5f);

            b.Ui.KnowledgeScrollBySlots(99);
            Check($"it clamps at the end rather than scrolling into nothing ({b.Ui.KnowledgeScrollOffset} of {range})",
                  Mathf.Abs(b.Ui.KnowledgeScrollOffset - range) < 0.5f);

            Check("and the left fade appears once there is something behind you",
                  UiProbe.IsDisplayed(UiNames.KnowledgeFadeLeft));
            Check("while the right fade is gone at the end",
                  !UiProbe.IsDisplayed(UiNames.KnowledgeFadeRight));

            b.Ui.KnowledgeScrollBySlots(-99);
            Check($"and it clamps at the front too ({b.Ui.KnowledgeScrollOffset})",
                  b.Ui.KnowledgeScrollOffset <= 0.5f);
            Check("with the right fade back and the left one gone",
                  UiProbe.IsDisplayed(UiNames.KnowledgeFadeRight)
                  && !UiProbe.IsDisplayed(UiNames.KnowledgeFadeLeft));
        }
    }

    // ================================================================
    // 9. The rung labels, which had drifted by one for ten sprints.
    // ================================================================
    private static void RunRungsAreLabelledByCapability()
    {
        Debug.Log("[Sprint35Verification] --- The ladder says what it means ---");
        SpeciesRoster.ResetToDefaults();

        // Sprint 25 moved Complement off the ladder and KnowledgeView's
        // positional label array was not updated, so the 70% rung -- which
        // is Secretory IgA -- was labelled "Complement", in the expanded
        // ladder and in the unlock ribbon both. The fix derives the label
        // from the capability; this check is the one that would have caught
        // it, and it is written against the TEXT the player reads.
        using (var b = new Booted())
        {
            var iga = KnowledgeLadder.Rungs[KnowledgeLadder.Rungs.Length - 1];
            Check($"the last rung is still Secretory IgA at {iga.ThresholdPercent}%",
                  iga.Capability == KnowledgeCapability.SecretoryIgA);

            // The expanded ladder is built for a species on click; open it
            // by driving the same path a click does is not reachable from
            // here, so the labels are checked where they are authored --
            // via the whole tree's text, which includes the expanded rows
            // that are built at construction time.
            var names = UiProbe.Names();
            Check($"the knowledge card is built with its rows ({names.Count} named elements)", names.Count > 0);

            string tree = UiProbe.Dump(20);
            Check("the ladder no longer offers Complement as a learnable rung",
                  !tree.Contains("Coated pathogens are damaged with no cell present"));
            Check("and it names Secretory IgA",
                  tree.Contains("Secretory IgA") || tree.Contains("Antibody in the lumen"));
        }

        // The applicability flag has a real shipped subject.
        Check("Vibrio cholerae is marked as never entering a cell",
              SpeciesRoster.Get(PathogenSpecies.VibrioCholerae).NeverEntersCells);
        int neverEnter = 0;
        foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
            if (SpeciesRoster.Get(sp).NeverEntersCells) neverEnter++;
        Check($"and it is the only organism on the board that does ({neverEnter})", neverEnter == 1);
    }

    // ================================================================
    // 10. The V. cholerae correction, measured on the board.
    // ================================================================
    private static void RunVibrioRunsStraight()
    {
        Debug.Log("[Sprint35Verification] --- It swims, it does not steer ---");
        SpeciesRoster.ResetToDefaults();
        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();
        InvasionTuning.AdhesionChanceAtWall = 0f;   // nothing sticks: measure movement only

        Check("Vibrio cholerae is marked as a straight swimmer",
              SpeciesRoster.Get(PathogenSpecies.VibrioCholerae).SwimsStraight);
        Check("and the generic bacterium is not, so the two can be compared",
              !SpeciesRoster.Get(PathogenSpecies.GenericLargeBacterium).SwimsStraight);

        float vibrioDrift = MeanWallwardDrift(PathogenSpecies.VibrioCholerae);
        float genericDrift = MeanWallwardDrift(PathogenSpecies.GenericLargeBacterium);

        Check($"a steering bacterium drifts toward the wall ({genericDrift:F2} cells)", genericDrift > 0.25f);
        // NOT |drift| < epsilon, which is how this check was first written
        // and why it failed on a correct implementation. An unbiased walk
        // that starts near the wall drifts slightly AWAY from it -- there
        // is more channel in that direction -- so a measured -0.30 is the
        // signature of a swimmer that does not steer, not a fault. The
        // claim is "does not aim at the wall": assert that.
        Check($"Vibrio does not aim at it ({vibrioDrift:F2} cells)", vibrioDrift < 0.1f);
        Check($"so the steerer ends up closer to the wall than the swimmer ({genericDrift:F2} > {vibrioDrift:F2})",
              genericDrift > vibrioDrift);

        InvasionTuning.ResetToDefaults();
        RoundClock.Reset();
    }

    /// <summary>
    /// How far toward the wall a cohort of one species moves, in coarse
    /// cells, averaged over many organisms and many steps. Measured through
    /// the real agent rather than by reading the flag back out: the flag is
    /// not the claim, the movement is.
    /// </summary>
    private static float MeanWallwardDrift(PathogenSpecies species)
    {
        var boardGo = new GameObject($"S35_drift_{species}");
        var board = boardGo.AddComponent<BoardConfig>();
        board.ConfigureForTest(25, 10, BoardAxis.Horizontal, AxisEnd.Negative, 6, 6, AxisEnd.Positive);
        var grid = new TissueGrid(board);
        var tally = new InvasionTally();
        var gut = new GutWall(board, grid, tally);
        RoundClock.Frozen = false;

        float total = 0f;
        const int Cohort = 60;
        for (int i = 0; i < Cohort; i++)
        {
            var go = new GameObject("drifter");
            go.AddComponent<SpriteRenderer>();
            var a = go.AddComponent<PathogenAgent>();
            a.Initialize(board, grid, gut, tally, null, null, speciesOverride: species);

            int start = board.LumenDepthFromInterface(a.CurrentCoarse);
            float t = 0f;
            for (int step = 0; step < 120 && a.State == PathogenState.Lumen; step++)
            {
                t += InvasionTuning.LumenStepIntervalSeconds;
                a.SimulationTick(InvasionTuning.LumenStepIntervalSeconds, t);
            }
            int end = board.LumenDepthFromInterface(a.CurrentCoarse);
            if (start >= 0 && end >= 0) total += start - end;   // positive = moved wallward
            Object.DestroyImmediate(go);
        }

        Object.DestroyImmediate(boardGo);
        return total / Cohort;
    }
}
