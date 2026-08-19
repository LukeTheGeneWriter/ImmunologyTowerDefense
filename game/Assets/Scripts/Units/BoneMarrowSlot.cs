using UnityEngine;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// One clickable bone marrow slot. Click detection uses Unity's legacy
    /// OnMouseDown message (backed by a BoxCollider2D and the physics
    /// raycast Unity runs against the main camera for it automatically) --
    /// deliberately not uGUI/EventSystem, since this project has no UI
    /// package installed (see docs/ENGINE_STATUS.md). OnMouseDown needs
    /// nothing beyond a Camera tagged MainCamera and a Collider2D, both of
    /// which GameBootstrap already provides.
    /// </summary>
    public class BoneMarrowSlot : MonoBehaviour
    {
        public int Index { get; private set; }
        private BoneMarrowManager manager;

        public void Init(BoneMarrowManager owner, int index)
        {
            manager = owner;
            Index = index;
        }

        private void OnMouseDown()
        {
            manager.OnSlotClicked(Index);
        }
    }
}
