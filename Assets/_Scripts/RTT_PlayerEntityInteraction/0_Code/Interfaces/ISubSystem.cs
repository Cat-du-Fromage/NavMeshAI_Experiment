using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public interface ISubSystem<T>
    {
        public IMainSystem<T> MainSystem { get; set; }
        public void AttachSubSystemTo(IMainSystem<T> mainSystem) //When on awake we will call this! ON THE MEDIATOR!
        {
            MainSystem = mainSystem;
        }
    }

    public interface ISelector<T> : ISubSystem<T>
    {
        
    }

    public interface IPlacement<T> : ISubSystem<T>
    {
        
    }
    
    public interface IEntityGroup<T> : ISubSystem<T>
    {
        
    }
}
