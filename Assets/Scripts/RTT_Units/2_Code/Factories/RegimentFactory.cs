using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public interface IFactory
    {
        public RegimentType GetRegimentType();
    }
    public class RegimentFactory : MonoBehaviour
    {
        //for now only ONE type : fusilier
        //but we want to extend later
        //How will we choose regiment?
        //- Name(string)?
        //- Enum?
        //- Jason Reflection abstractClass? https://www.youtube.com/watch?v=FGVkio4bnPQ
        
        
        //On Construction: Instanciate Regiment + set RegimentType
        
    }
}