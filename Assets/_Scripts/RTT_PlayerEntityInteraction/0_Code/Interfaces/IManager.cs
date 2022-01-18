using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public interface IManager<T>
    {
        public IMediator<T> Mediator { get; set; }
        public void SetMediator(IMediator<T> mediator) //When on awake we will call this! ON THE MEDIATOR!
        {
            Mediator = mediator;
        }
    }

    public interface ISelector<T> : IManager<T>
    {
        
    }

    public interface IPlacement<T> : IManager<T>
    {
        
    }
    
    public interface IEntityGroup<T> : IManager<T>
    {
        
    }
}
