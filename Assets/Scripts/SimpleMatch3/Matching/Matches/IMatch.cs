using System;
using System.Collections.Generic;
using SimpleMatch3.Matching.Data;

namespace SimpleMatch3.Matching.Matches
{
    public interface IMatch : IComparable<IMatch>
    {
        public MatchData Data { get; }
        public bool IsMatch(Tile.Tile startingTile, IEnumerable<Tile.Tile> floodFilledTiles, out MatchCoordinateOffsets participants);
    }
}