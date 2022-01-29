using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public interface IEntitySystem<T>
    {
        public void Notify(IEntitySubSystem<T> subSystem, T entity);
    }

    public interface IEntitySubSystem<T>
    {
        public IEntitySystem<T> MainSystem { get; set; }
        public void AttachSubSystemTo(IEntitySystem<T> mainSystem) //When on awake we will call this! ON THE MEDIATOR!
        {
            MainSystem = mainSystem;
        }
    }
}