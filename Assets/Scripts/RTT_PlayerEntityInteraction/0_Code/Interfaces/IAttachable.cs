using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public interface IAttachable<in T2>
        where T2 : MonoBehaviour
    {
        public void AttachTo(T2 parentEntity);
    }
    
}
