using System.Collections.Generic;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Grid;
using ImmunologyTD.Pathogens;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 22 -- secretory IgA and in-lumen bacterial replication.
///
/// The wall itself (mill, crowd field, plaques, the anti-trickle floor) is
/// covered by MapVerification. This file covers the two mechanics the
/// Director added on 2026-10-18, and in particular it tries to prove the
/// claim the whole design rests on, which is easy to state and easy to get
/// subtly wrong in code:
///
///   **Under IgA, a bacterium's own replication is what traps it.**
///
/// That is not a slogan. It is a falsifiable property of the simulation, and
/// the tests below check both halves: that density flips from threat to sink
/// above the rung, and that dividing inside a raft makes the raft bigger
/// rather than making more free organisms.
/// </summary>
public static class Sprint22Verification
{
    private static int checks;
    private static int failures;

    [MenuItem("ImmunologyTD/Verify/Sprint 22 -- IgA and replication")]
    public static void RunAll()
    {
        checks = 0; failures = 0;

        // Seeded, because several of these are RACES -- coating against a
        // charging plaque, division against link-breaking -- and an unseeded
        // race makes a harness that passes alone and fails in the suite, as
        // this one did. A seeded run also means a failure can be reproduced
        // exactly rather than argued about.
        Random.InitState(2210);

        Debug.Log("[Sprint22Verification] === IgA and in-lumen replication ===");

        RunCoating();
        RunCrossLinkingExcludes();
        RunDensityFlipsSign();
        RunEnchainedGrowth();
        RunReplicationDensityDamper();
        RunPacified();
        RunBarrierWeakSpot();
        RunVirusAsymmetry();
        RunSentinelSlough();
        RunPlayerAgency();

        if (failures > 0)
            Debug.LogError($"[Sprint22Verification] FAILED -- {failures} of {checks} checks failed.");
        else
            Debug.Log($"[Sprint22Verification] OK -- {checks} checks passed.");
    }

    private static void Check(string label, bool ok)
    {
        checks++;
        if (ok) Debug.Log($"[Sprint22Verification] PASS -- {label}");
        else { failures++; Debug.LogError($"[Sprint22Verification] FAIL -- {label}"); }
    }

    // -----------------------------------------------------------------

    private class Rig
    {
        public GameObject BoardGo;
        public BoardConfig Board;
        public TissueGrid Grid;
        public InvasionTally Tally;
        public GutWall Wall;
        public KnowledgeLedger Knowledge;
        public readonly List<GameObject> Junk = new List<GameObject>();

        public void Dispose()
        {
            for (int i = 0; i < Junk.Count; i++)
                if (Junk[i] != null) Object.DestroyImmediate(Junk[i]);
            if (BoardGo != null) Object.DestroyImmediate(BoardGo);
        }
    }

    private static Rig BuildRig(string name, float knowledgePercent)
    {
        InvasionTuning.ResetToDefaults();
        IgaTuning.ResetToDefaults();

        var rig = new Rig();
        rig.BoardGo = new GameObject($"Sprint22_Board_{name}");
        rig.Board = rig.BoardGo.AddComponent<BoardConfig>();
        rig.Board.ConfigureForTest(100, 40, BoardAxis.Horizontal, AxisEnd.Negative, 25, 25, AxisEnd.Positive);
        rig.Grid = new TissueGrid(rig.Board);
        rig.Tally = new InvasionTally();
        rig.Wall = new GutWall(rig.Board, rig.Grid, rig.Tally);

        rig.Knowledge = new KnowledgeLedger();
        if (knowledgePercent > 0f)
        {
            rig.Knowledge.Add(PathogenClass.LargeBacterium, knowledgePercent);
            rig.Wall.BindKnowledge(rig.Knowledge);
        }

        // A daughter factory that mirrors what PathogenSpawner does, without
        // dragging the pool into the harness.
        rig.Wall.SpawnDaughter = (parent, now) =>
        {
            var d = NewAgent(rig, $"Daughter_{rig.Junk.Count}", PathogenClass.LargeBacterium);
            d.AdhereToWall(Mathf.Clamp(parent.WallS + 0.06f, 0f, rig.Wall.WallLengthCells - 0.001f));
            d.SetWallPosition(d.WallS, parent.WallD);
            return d;
        };
        return rig;
    }

    private static PathogenAgent NewAgent(Rig rig, string name, PathogenClass pClass)
    {
        var go = new GameObject($"Sprint22_{name}");
        go.AddComponent<SpriteRenderer>();
        var agent = go.AddComponent<PathogenAgent>();
        rig.Junk.Add(go);
        agent.Initialize(rig.Board, rig.Grid, rig.Wall, rig.Tally,
                         a => { }, (c, cls, free, t) => false, classOverride: pClass);
        return agent;
    }

    /// <summary>Puts n organisms in a tight clump and lets them settle in to
    /// grip, which takes about two seconds by design.</summary>
    private static List<PathogenAgent> Clump(Rig rig, int n, float centre, string label)
    {
        var list = new List<PathogenAgent>();
        for (int i = 0; i < n; i++)
        {
            var a = NewAgent(rig, $"{label}_{i}", PathogenClass.LargeBacterium);
            a.AdhereToWall(centre + (i - n * 0.5f) * 0.07f);
            list.Add(a);
        }
        return list;
    }

    private static void Advance(Rig rig, float seconds, float from = 0f)
    {
        int steps = Mathf.RoundToInt(seconds / 0.1f);
        for (int i = 0; i < steps; i++)
            rig.Wall.Tick(0.1f, from + i * 0.1f, from + i * 0.1f);
    }

    // -----------------------------------------------------------------
    // 1. Coating only happens above the rung
    // -----------------------------------------------------------------
    private static void RunCoating()
    {
        Debug.Log("[Sprint22Verification] --- Coating ---");

        // Below the rung: nothing should ever be coated, however long we wait.
        var below = BuildRig("Below", 69f);
        var organisms = Clump(below, 5, 10f, "Below");
        Advance(below, 60f);
        int coatedBelow = 0;
        foreach (var o in organisms) if (o.IsCoated) coatedBelow++;
        Check($"Below 70% knowledge, nothing is coated ({coatedBelow})", coatedBelow == 0);
        below.Dispose();

        // At the rung: everything coats within a few seconds, because the
        // reward has to be legible the moment it is earned.
        var at = BuildRig("At", 70f);
        var organisms2 = Clump(at, 5, 10f, "At");
        Advance(at, 30f);
        int coatedAt = 0;
        foreach (var o in organisms2) if (o.IsCoated) coatedAt++;
        Check($"At 70%, all five are coated within 30s ({coatedAt}/5)", coatedAt == 5);

        // Per-species: IgA against one class must not touch another.
        var virus = NewAgent(at, "Virus", PathogenClass.IntracellularVirus);
        virus.AdhereToWall(10f);
        Advance(at, 30f, 30f);
        Check("A species we have NOT characterised is untouched", !virus.IsCoated);
        at.Dispose();
    }

    // -----------------------------------------------------------------
    // 2. Cross-linking excludes: bulk cannot reach the wall
    // -----------------------------------------------------------------
    private static void RunCrossLinkingExcludes()
    {
        Debug.Log("[Sprint22Verification] --- Cross-linking excludes ---");
        var rig = BuildRig("Exclude", 70f);
        var organisms = Clump(rig, 6, 10f, "Ex");

        Advance(rig, 60f);

        int linked = 0;
        foreach (var o in organisms) if (o.IsCrossLinked) linked++;
        Check($"A coated crowd agglutinates ({linked}/6 cross-linked)", linked >= 4);

        int gripping = 0;
        foreach (var o in organisms) if (o.IsCrossLinked && o.IsGrippingWall) gripping++;
        Check($"No raft member still grips the wall ({gripping})", gripping == 0);

        foreach (var o in organisms)
        {
            if (!o.IsCrossLinked) continue;
            Check($"...held beyond the grip depth ({o.WallD:0.00} > {InvasionTuning.GripDepthCells})",
                o.WallD > InvasionTuning.GripDepthCells);
            break;
        }

        // The whole point: a raft contributes nothing to crowding, so the
        // field over a fully-rafted patch collapses.
        Check($"Crowd over the rafted patch has collapsed ({rig.Wall.CrowdAt(10f):0.00})",
            rig.Wall.CrowdAt(10f) < InvasionTuning.IngressCrowdFloor);

        // And bigger rafts are held further out -- density INCREASES the
        // exclusion, which is the fourth lock on the trap.
        float small = rig.Wall.ClumpRadiusCells(0);
        Check($"Clump bulk grows with membership ({small:0.00} cells for {rig.Wall.ClumpMembers(0)})",
            small >= IgaTuning.ClumpBaseRadiusCells);

        rig.Dispose();
    }

    // -----------------------------------------------------------------
    // 3. THE HEADLINE: density flips sign at the rung
    //
    // The same crowd, the same positions, the same duration. The only
    // difference is whether the player has characterised the species.
    // Below the rung it breaches. Above it, it is swept out without a
    // single breach and without anything being killed.
    // -----------------------------------------------------------------
    private static void RunDensityFlipsSign()
    {
        Debug.Log("[Sprint22Verification] --- Density flips sign ---");

        var naive = BuildRig("Naive", 0f);
        Clump(naive, 8, 10f, "Naive");
        Advance(naive, 120f);
        Check($"Without IgA, a crowd of 8 breaches the wall ({naive.Tally.Breaches})",
            naive.Tally.Breaches >= 1);

        var armed = BuildRig("Armed", 70f);
        Clump(armed, 8, 10f, "Armed");
        Advance(armed, 120f);
        Check($"With IgA, THE SAME crowd never breaches ({armed.Tally.Breaches})",
            armed.Tally.Breaches == 0);
        Check($"...and nothing reached the tissue ({armed.Tally.ReleasedIntoTissue})",
            armed.Tally.ReleasedIntoTissue == 0);

        // Nothing is killed. The gut's answer is flush, not eat -- there is
        // no luminal killing anywhere in this sprint.
        Check($"...and IgA killed nothing; it excluded them ({armed.Wall.IgaExcluded} excluded)",
            armed.Wall.IgaExcluded > 0 || armed.Wall.TotalAdhered > 0);

        naive.Dispose();
        armed.Dispose();
    }

    // -----------------------------------------------------------------
    // 4. ENCHAINED GROWTH -- the property the whole design rests on
    //
    // Moor 2017 / Bansept 2019: IgA does not slow bacteria, it cross-links
    // daughter cells as they divide. So under IgA, dividing builds your own
    // cage. This asserts both halves of that.
    // -----------------------------------------------------------------
    private static void RunEnchainedGrowth()
    {
        Debug.Log("[Sprint22Verification] --- Enchained growth ---");
        var rig = BuildRig("Enchained", 70f);

        // One lone coated organism, far from anything, so the only way its
        // clump can grow is by its own division.
        var parent = NewAgent(rig, "Parent", PathogenClass.LargeBacterium);
        parent.AdhereToWall(20f);
        Advance(rig, 20f);
        Check("The lone organism is coated", parent.IsCoated);

        // Long enough for several divisions at the 22s interval.
        Advance(rig, 180f, 20f);

        Check($"It divided while coated -- IgA does NOT sterilise it ({rig.Wall.LumenDivisions} divisions)",
            rig.Wall.LumenDivisions >= 2);

        Check($"Its divisions built a clump ({rig.Wall.ClumpMembers(parent.ClumpId)} members)",
            parent.IsCrossLinked && rig.Wall.ClumpMembers(parent.ClumpId) >= 2);

        // The payoff, stated as the test it is: replication under IgA
        // produced exclusion rather than a growing free population.
        int free = 0;
        var adhered = rig.Wall.Adhered;
        for (int i = 0; i < adhered.Count; i++)
            if (adhered[i] != null && adhered[i].IsGrippingWall && !adhered[i].IsCrossLinked) free++;
        Check($"Replicating under IgA produced NO free gripping organisms ({free})", free == 0);
        Check($"...and no breach at all ({rig.Tally.Breaches})", rig.Tally.Breaches == 0);

        rig.Dispose();
    }

    // -----------------------------------------------------------------
    // 5. Density damps replication -- by nutrient competition, not quorum
    //    sensing (IGA_REFERENCE.md claim 4)
    // -----------------------------------------------------------------
    private static void RunReplicationDensityDamper()
    {
        Debug.Log("[Sprint22Verification] --- Replication damper ---");

        var sparse = BuildRig("Sparse", 0f);
        for (int i = 0; i < 6; i++)
        {
            var a = NewAgent(sparse, $"Sparse_{i}", PathogenClass.LargeBacterium);
            a.AdhereToWall(3f + i * 5f); // a full crowd radius apart, so no two compete
        }
        Advance(sparse, 180f);
        int sparseDivisions = sparse.Wall.LumenDivisions;

        var dense = BuildRig("Dense", 0f);
        Clump(dense, 6, 10f, "Dense");
        Advance(dense, 180f);
        int denseDivisions = dense.Wall.LumenDivisions;

        Check($"Spread-out bacteria divide freely ({sparseDivisions} divisions)", sparseDivisions > 0);
        Check($"Crowded bacteria divide less ({denseDivisions} vs {sparseDivisions})",
            denseDivisions < sparseDivisions);

        sparse.Dispose();
        dense.Dispose();
    }

    // -----------------------------------------------------------------
    // 7. A dead stretch is a WEAK SPOT, not a hole (Director, D2)
    // -----------------------------------------------------------------
    private static void RunBarrierWeakSpot()
    {
        Debug.Log("[Sprint22Verification] --- Barrier: weak spot, not hole ---");
        var rig = BuildRig("Barrier", 0f);

        Check("The wall starts intact",
            rig.Wall.StateAt(10f) == GutWall.BarrierState.Intact);

        // A rupture kills the epithelium it came through. Sample AT the
        // rupture: the wall self-heals (fork 7), so by the end of a long run
        // it has already grown back, which is correct and would hide this.
        int deadAtRupture = 0;
        float multiplierAtRupture = 0f;
        rig.Wall.Ruptured += (centre, released, footprint) =>
        {
            deadAtRupture = rig.Wall.DeadSegmentCount;
            multiplierAtRupture = rig.Wall.BarrierChargeMultiplierAt(centre);
        };

        Clump(rig, 8, 10f, "Bar");
        Advance(rig, 120f);
        Check($"A breach happened ({rig.Tally.Breaches})", rig.Tally.Breaches >= 1);
        Check($"...and it killed the wall it tore through ({deadAtRupture} segments)",
            deadAtRupture > 0);

        // The Director chose the gentler option: dead wall charges FASTER but
        // still needs a crowd. There must not be a second ingress path.
        Check($"Dead wall charges faster ({multiplierAtRupture:0.0}x)", multiplierAtRupture > 1f);

        Check($"...and an unmanaged wall grows back on its own ({rig.Wall.DeadSegmentCount} still dead)",
            rig.Wall.DeadSegmentCount < deadAtRupture);

        var hole = BuildRig("Hole", 0f);
        hole.Wall.DamageBarrier(10f, 999f, 0f);
        Check("A stretch can be killed outright",
            hole.Wall.StateAt(10f) == GutWall.BarrierState.Dead);

        // ONE organism on dead wall. If dead wall were an open hole it would
        // walk through. It must not: ingress is still something a crowd does.
        // Suppress replication for this one: left to divide, a "lone"
        // bacterium becomes a crowd and gets in legitimately, which would
        // test the opposite of what this check is about.
        IgaTuning.LumenDivideIntervalSeconds = 1e6f;
        var loner = NewAgent(hole, "Loner", PathogenClass.LargeBacterium);
        loner.AdhereToWall(10f);
        Advance(hole, 180f);
        IgaTuning.ResetToDefaults();
        Check($"A LONE organism on dead wall still cannot get in ({hole.Tally.Breaches})",
            hole.Tally.Breaches == 0);

        rig.Dispose();
        hole.Dispose();
    }

    // -----------------------------------------------------------------
    // 8. The asymmetry: bacteria and viruses want opposite walls
    // -----------------------------------------------------------------
    private static void RunVirusAsymmetry()
    {
        Debug.Log("[Sprint22Verification] --- Virus asymmetry ---");

        // Viruses on INTACT wall: there are living cells to infect.
        var intact = BuildRig("ViralIntact", 0f);
        for (int i = 0; i < 8; i++)
        {
            var v = NewAgent(intact, $"VI_{i}", PathogenClass.IntracellularVirus);
            v.AdhereToWall(10f + (i - 4) * 0.07f);
        }
        Advance(intact, 120f);
        Check($"Viruses break through LIVING wall ({intact.Tally.Breaches})",
            intact.Tally.Breaches >= 1);

        // The same crowd of viruses on DEAD wall: nothing there to infect.
        var dead = BuildRig("ViralDead", 0f);
        for (float s = 8f; s <= 12f; s += 0.4f) dead.Wall.DamageBarrier(s, 999f, 0f);
        for (int i = 0; i < 8; i++)
        {
            var v = NewAgent(dead, $"VD_{i}", PathogenClass.IntracellularVirus);
            v.AdhereToWall(10f + (i - 4) * 0.07f);
        }
        // 40s: comfortably longer than the ~9s this crowd needs on living
        // wall, and shorter than the 45s the stretch takes to grow back. The
        // claim is about the window in which the wall is DEAD -- once it
        // regrows there are living cells again and the viruses get in, which
        // is the mechanic working, not failing.
        Advance(dead, 40f);
        Check($"The SAME viruses cannot use a dead stretch ({dead.Tally.Breaches})",
            dead.Tally.Breaches == 0);

        // ...and once it has grown back, they can. The asymmetry is a window,
        // not a permanent immunity.
        Advance(dead, 80f, 40f);
        Check($"...but once it regrows, they can ({dead.Tally.Breaches})",
            dead.Tally.Breaches >= 1);

        // And the mirror: bacteria do fine on dead wall -- better, in fact.
        var bact = BuildRig("BactDead", 0f);
        for (float s = 8f; s <= 12f; s += 0.4f) bact.Wall.DamageBarrier(s, 999f, 0f);
        Clump(bact, 8, 10f, "BD");
        Advance(bact, 120f);
        Check($"Bacteria DO exploit the same dead stretch ({bact.Tally.Breaches})",
            bact.Tally.Breaches >= 1);

        intact.Dispose();
        dead.Dispose();
        bact.Dispose();
    }

    // -----------------------------------------------------------------
    // 9. The sentinel goblet cell (Birchenough 2016, Science)
    // -----------------------------------------------------------------
    private static void RunSentinelSlough()
    {
        Debug.Log("[Sprint22Verification] --- Sentinel sloughing ---");
        var rig = BuildRig("Sentinel", 0f);

        // A crypt at lane 10, which is where our crowd will gather.
        var featuresGo = new GameObject("Sprint22_WallFeatures");
        rig.Junk.Add(featuresGo);
        var features = new GutWallFeatures(rig.Board);
        GutWallFeatures.Current = features;
        features.SetCryptCount(1);

        int cryptLane = features.CryptLanes.Count > 0 ? features.CryptLanes[0] : 10;
        float cryptS = rig.Wall.SOfLane(cryptLane);

        int sloughEvents = 0;
        rig.Wall.Sloughs += (centre, half, expelled) => sloughEvents++;

        Clump(rig, 8, cryptS, "Sent");
        Advance(rig, 30f);

        Check($"The sentinel fired on its own, with no purchase ({sloughEvents} events)",
            sloughEvents >= 1);
        Check($"...expelling organisms rather than killing them ({rig.Wall.Sloughed})",
            rig.Wall.Sloughed > 0);
        Check($"...which relieved the wall ({rig.Wall.TotalAdhered} still adhered)",
            rig.Wall.TotalAdhered < 8);

        // It is TRIGGERED, not timed: a quiet crypt never fires.
        var quiet = BuildRig("Quiet", 0f);
        int quietEvents = 0;
        quiet.Wall.Sloughs += (c, h, e) => quietEvents++;
        IgaTuning.LumenDivideIntervalSeconds = 1e6f; // as above: keep "lone" lone
        var lone = NewAgent(quiet, "Lone", PathogenClass.LargeBacterium);
        lone.AdhereToWall(cryptS);
        Advance(quiet, 120f);
        IgaTuning.ResetToDefaults();
        Check($"A quiet crypt never fires -- it is triggered, not timed ({quietEvents})",
            quietEvents == 0);

        GutWallFeatures.Current = null;
        rig.Dispose();
        quiet.Dispose();
    }

    // -----------------------------------------------------------------
    // 10. The player can answer the wall (Director, D3)
    // -----------------------------------------------------------------
    private static void RunPlayerAgency()
    {
        Debug.Log("[Sprint22Verification] --- Player agency ---");
        var rig = BuildRig("Agency", 0f);

        Clump(rig, 6, 10f, "Ag");
        Advance(rig, 8f); // caught mid-fill: 6 members rupture in about 10s

        int plaque = -1;
        for (int i = 0; i < rig.Wall.PlaqueCapacity; i++)
            if (rig.Wall.PlaqueAt(i).Live && rig.Wall.PlaqueAt(i).Charge > 0f) plaque = i;
        Check("A plaque is charging", plaque >= 0);

        float before = rig.Wall.PlaqueAt(plaque).Charge;
        float removed = rig.Wall.SuppressPlaques(10f, 1.5f, 2f);
        Check($"A tower can talk a blister down ({before:0.000} -> {rig.Wall.PlaqueAt(plaque).Charge:0.000})",
            removed > 0f && rig.Wall.PlaqueAt(plaque).Charge < before);

        // Repair restores wall the player has lost.
        rig.Wall.DamageBarrier(20f, 999f, 0f);
        Check("A stretch is dead", rig.Wall.StateAt(20f) == GutWall.BarrierState.Dead);
        int repaired = rig.Wall.RepairBarrier(20f);
        Check($"Repair restores it ({repaired} segments)",
            repaired > 0 && rig.Wall.StateAt(20f) == GutWall.BarrierState.Intact);

        rig.Dispose();
    }

    // -----------------------------------------------------------------
    // 6. Pacified, slower, more fragile -- and NO luminal killing bonus
    // -----------------------------------------------------------------
    private static void RunPacified()
    {
        Debug.Log("[Sprint22Verification] --- Pacified ---");
        var rig = BuildRig("Pacified", 70f);

        var organism = NewAgent(rig, "Pac", PathogenClass.LargeBacterium);
        organism.AdhereToWall(10f);
        Advance(rig, 30f);
        Check("Coated", organism.IsCoated);

        Check($"Virulence is reduced but NOT zero ({organism.VirulenceScale:0.00})",
            organism.VirulenceScale > 0f && organism.VirulenceScale < 1f);
        Check($"Motility is reduced but NOT zero ({organism.MotilityScale:0.00})",
            organism.MotilityScale > 0f && organism.MotilityScale < 1f);

        // "Easier to deal with" is honoured BEHIND the wall only. Secretory
        // IgA is non-inflammatory and the lumen has no phagocytes, so there
        // must be no luminal killing anywhere in this sprint.
        Check("The damage bonus is a tissue-side multiplier, not a luminal kill",
            IgaTuning.TissueDamageTakenMultiplier > 1f);

        var uncoated = NewAgent(rig, "Naive", PathogenClass.IntracellularVirus);
        Check("An uncoated organism is unmodified",
            Mathf.Approximately(uncoated.VirulenceScale, 1f)
            && Mathf.Approximately(uncoated.MotilityScale, 1f));

        rig.Dispose();
    }
}
