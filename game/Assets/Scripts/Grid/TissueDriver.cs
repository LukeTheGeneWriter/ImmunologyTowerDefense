using UnityEngine;

namespace ImmunologyTD.Grid
{
    /// <summary>
    /// Forwards the real clock into <see cref="TissueGrid.Tick"/> so debris
    /// dissipation and host-cell regrowth advance in a running build.
    ///
    /// It exists as its own three-line MonoBehaviour rather than as a call
    /// inside PathogenSpawner.Tick for one reason: the host layer is not the
    /// pathogens' business. Tissue keeps healing whether or not anything is
    /// invading, and a later sprint that touches the pathogen spawner should
    /// not be able to stop tissue from recovering by accident.
    ///
    /// Every verification harness calls TissueGrid.Tick directly with a
    /// simulated clock instead -- nothing about the host layer is reachable
    /// only through Update().
    /// </summary>
    public class TissueDriver : MonoBehaviour
    {
        private TissueGrid tissueGrid;

        public void Bind(TissueGrid tissueGrid)
        {
            this.tissueGrid = tissueGrid;
        }

        private void Update()
        {
            if (tissueGrid == null) return;
            tissueGrid.Tick(Time.deltaTime, Time.time);
        }
    }
}
