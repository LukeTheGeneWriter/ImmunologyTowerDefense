using UnityEngine;
using ImmunologyTD.Grid;
using ImmunologyTD.Units;
using ImmunologyTD.Rendering;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// A helper-T cell resident in the lymph node (GAME_DESIGN.md §5a/§5c).
    /// Emitted by the helper-T progenitor, it wanders the node biased toward
    /// the co-localisation signal (the same <see cref="Chemotaxis"/> path the
    /// tissue units use), and **is born with a random 8-bit antigen
    /// <see cref="Tag"/>** -- the barcode a dendritic cell's cargo is checked
    /// against when they pair.
    ///
    /// It ages out after <see cref="AdaptiveTuning.LymphocyteLifespanSeconds"/>
    /// and the progenitor emits a fresh one with a new tag: that is what
    /// makes the node's barcode repertoire **turn over** (§5c step 6), so a
    /// player with no current match is not permanently stuck.
    ///
    /// Movement is driven by <see cref="LymphNode.Tick"/> calling
    /// <see cref="NodeTick"/> -- explicit time, like every other agent in
    /// this project. Update() only runs the visual tween.
    /// </summary>
    public class Lymphocyte : MonoBehaviour
    {
        private LymphNode node;
        private SpriteRenderer sr;
        private System.Action<Lymphocyte> onDespawn;

        private readonly FineCoord[] candidateBuffer = new FineCoord[4];
        private readonly float[] weightBuffer = new float[4];

        /// <summary>This cell's fixed 8-bit antigen receptor. Set at birth,
        /// never re-rolled. A DC:helper-T pairing teaches iff
        /// <see cref="Antigen.IsMatch"/> of this and the DC's cargo.</summary>
        public byte Tag { get; private set; }

        /// <summary>Node-local fine-tile position.</summary>
        public FineCoord Node { get; private set; }

        public float BornAt { get; private set; }

        /// <summary>Set by <see cref="LymphNode"/> while this cell is locked
        /// in a pairing -- it stops moving for
        /// <see cref="AdaptiveTuning.PairingSeconds"/> whether or not the
        /// pairing will teach anything (§5c step 5: the freeze is the cost).</summary>
        public bool Frozen { get; set; }

        private static readonly Color RestColor = new Color(0.32f, 0.72f, 0.70f);   // helper-T teal
        private static readonly Color PairedColor = new Color(0.82f, 0.94f, 0.92f); // near-white while paired

        private Vector3 tweenStart;
        private Vector3 tweenEnd;
        private float tweenTimer;

        public void Initialize(LymphNode node, byte tag, FineCoord start, float bornAt,
            System.Action<Lymphocyte> onDespawn)
        {
            this.node = node;
            this.onDespawn = onDespawn;
            Tag = tag;
            Node = start;
            BornAt = bornAt;
            Frozen = false;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteShapes.Lymphocyte; // Sprint 13
            sr.color = RestColor;
            sr.enabled = true;
            sr.sortingOrder = 12;
            float s = node.AgentWorldSize;
            transform.localScale = new Vector3(s, s, 1f);

            tweenStart = tweenEnd = node.NodeToWorld(Node);
            transform.position = tweenStart;
            tweenTimer = Random.Range(0f, BoardConfig.TickIntervalSeconds);
        }

        /// <summary>One logical node tick: wander unless frozen. Called by
        /// <see cref="LymphNode.Tick"/> with the node's simulated clock.</summary>
        public void NodeTick(float currentTime)
        {
            if (node == null) return;

            tweenStart = transform.position;
            if (!Frozen)
            {
                Node = Chemotaxis.ChooseNextStep(
                    Node, node.NodeBoard, node.Coloc, true, candidateBuffer, weightBuffer);
            }
            tweenEnd = node.NodeToWorld(Node);
            if (sr != null) sr.color = Frozen ? PairedColor : RestColor;
        }

        private void Update()
        {
            if (node == null) return;
            if (ImmunologyTD.Rounds.RoundClock.Frozen) return; // Sprint 9: freeze mid-glide during the buy phase
            tweenTimer += Time.deltaTime;
            float t = Mathf.Clamp01(tweenTimer / BoardConfig.TickIntervalSeconds);
            transform.position = Vector3.Lerp(tweenStart, tweenEnd, t);
            if (tweenTimer >= BoardConfig.TickIntervalSeconds) tweenTimer -= BoardConfig.TickIntervalSeconds;
        }

        /// <summary>Return to the pool (lifespan expiry, or the round
        /// boundary). Routes through the emitter's callback so the node's
        /// resident list and the progenitor's child count both drop.</summary>
        public void DespawnToPool()
        {
            var cb = onDespawn;
            onDespawn = null;
            if (cb != null) cb(this);
            else { ResetForPool(); gameObject.SetActive(false); }
        }

        public void ResetForPool()
        {
            node = null;
            onDespawn = null;
            Frozen = false;
            tweenTimer = 0f;
            if (sr != null) { sr.color = RestColor; sr.enabled = true; }
        }
    }
}
