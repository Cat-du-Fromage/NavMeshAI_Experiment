using UnityEngine;

namespace KaizerWaldCode
{
    public abstract class InteractionSubSystem : MonoBehaviour
    {
        public Porto_InteractionSystem System { get; set; }
        
        public void AttachSubSystemTo(Porto_InteractionSystem system)
        {
            System = system;
        }

        public void Notify(Porto_InteractionSystem mainSystem)
        {
            
        }
    }
}