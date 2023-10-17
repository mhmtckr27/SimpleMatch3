using System;
using System.Collections.Generic;
using SimpleMatch3.Matching.Data;

namespace SimpleMatch3.Matching.Matches
{
    public abstract class MatchBase : IMatch
    {
        public MatchData Data { get; }
        
        protected MatchBase(MatchData data)
        {
            Data = data;
        }

        public abstract bool IsMatch(Tile.Tile startingTile, IEnumerable<Tile.Tile> floodFilledTiles, out MatchCoordinateOffsets participants);

        public int CompareTo(IMatch other)
        {
            return Data.Priority.CompareTo(other.Data.Priority);
        }
    }
}