using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleMatch3.Matching.Data
{
    [CreateAssetMenu(menuName = "ScriptableObjects/MatchData", fileName = "MatchData_")]
    public class MatchData : ScriptableObject
    {
        [field:SerializeField] public int ParticipantCount { get; private set; }
        [field:SerializeField] public int Priority { get; private set; }
        [field:SerializeField] public List<MatchCoordinateOffsets> MatchesCoordinateOffsets { get; private set; }
    }


    [Serializable]
    public class MatchCoordinateOffsets : IComparable<MatchCoordinateOffsets>
    {
        [field: SerializeField] public List<Vector2Int> Offsets;
        
        public int CompareTo(MatchCoordinateOffsets other)
        {
            if (Offsets.Any(offset => !other.Offsets.Contains(offset)))
                return -1;

            return 0;
        }
    }
}