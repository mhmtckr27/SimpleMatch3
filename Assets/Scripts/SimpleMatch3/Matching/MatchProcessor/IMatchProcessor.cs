using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleMatch3.Matching.Data;

namespace SimpleMatch3.Matching.MatchProcessor
{
    public interface IMatchProcessor
    {
        Task<List<MatchCoordinateOffsets>> ProcessMatches(Tile.Tile tile);
    }
}