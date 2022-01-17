using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    // The Mediator interface declares a method used by components to notify the
    // mediator about various events. The Mediator may react to these events and
    // pass the execution to other components.
    public interface IMediator
    {
        public void Notify(IManager sender, string ev);
    }

    public interface IManager
    {
        public IMediator Mediator { get; set; }
        public void SetMediator(IMediator mediator) //When on awake we will call this! ON THE MEDIATOR!
        {
            Mediator = mediator;
        }
    }
}
