using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public interface IMediator
    {
        public void Notify(IEntityMovement sender);
    }

    public interface IEntityMovement
    {
        
    }
}