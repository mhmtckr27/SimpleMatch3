using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleMatch3.Matching.Data;
using SimpleMatch3.Matching.Matches;

namespace SimpleMatch3.Matching.MatchProcessor
{
    public interface IMatchProcessor
    {
        Task<List<(IMatch, MatchCoordinateOffsets)>> ProcessMatches(Tile.Tile tile);
    }
}