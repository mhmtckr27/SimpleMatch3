using System.Collections.Generic;
using System.Linq;
using SimpleMatch3.Matching.Data;

namespace SimpleMatch3.Matching.Matches
{
    public abstract class SingleLineMatchBase : MatchBase
    {
        public override bool IsMatch(Tile.Tile startingTile, IEnumerable<Tile.Tile> floodFilledTiles, out MatchCoordinateOffsets participantsOffsets)
        {
            var tileCoords = floodFilledTiles.Select(tile => tile.Data.Coordinates - startingTile.Data.Coordinates);

            participantsOffsets = Data.MatchesCoordinateOffsets.FirstOrDefault(coordOffsets =>
                coordOffsets.Offsets.TrueForAll(coord => tileCoords.Contains(coord)));
            
            return participantsOffsets != null;
        }

        protected SingleLineMatchBase(MatchData data) : base(data)
        {
        }
    }
}