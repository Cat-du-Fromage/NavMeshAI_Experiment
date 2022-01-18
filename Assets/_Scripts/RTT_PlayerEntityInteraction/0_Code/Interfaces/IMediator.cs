using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    // The Mediator interface declares a method used by components to notify the
    // mediator about various events. The Mediator may react to these events and
    // pass the execution to other components.
    public interface IMediator<T>
    {
        //Selection
        public void NotifyEntitySelected(ISelector<T> sender, T entitySelected);
        public void NotifyClearSelections(ISelector<T> sender);
        //Placement
        public void NotifyDestinationSet(IPlacement<T> sender, Dictionary<int,Transform[]> keys);
        public void NotifyDisplayTokens(IPlacement<T> sender, bool enable);
    }
}