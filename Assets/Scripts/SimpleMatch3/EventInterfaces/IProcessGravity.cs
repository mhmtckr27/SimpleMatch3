using System.Collections.Generic;
using UnityEngine;

namespace SimpleMatch3.EventInterfaces
{
    public interface IProcessGravity
    {
        public class ProcessGravityForDrops
        {
            public List<Drop.Drop> Drops;
        }
    }
}