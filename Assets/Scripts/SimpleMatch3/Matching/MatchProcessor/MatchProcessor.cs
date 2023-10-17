using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleMatch3.Matching.Data;
using SimpleMatch3.Matching.Matches;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Matching.MatchProcessor
{
    public class MatchProcessor : IMatchProcessor
    {
        private Board.Board _board;
        private List<IMatch> _matches;

        public MatchProcessor(IInstantiator instantiator, Board.Board board)
        {
            _board = board;
            _matches = new List<IMatch>()
            {
                instantiator.Instantiate<SingleLineMatch3>(),
                instantiator.Instantiate<SingleLineMatch4>(),
                instantiator.Instantiate<SingleLineMatch5>()
            };
            
            _matches.Sort();
        }
        
        public async Task<List<MatchCoordinateOffsets>> ProcessMatches(Tile.Tile tile)
        {
            var participants = new MatchCoordinateOffsets();
            var foundMatch = false;
            var matches = new List<MatchCoordinateOffsets>();

            await Task.Run(() =>
            {
                var tiles = FloodFill(tile);

                //Filter out the matches that require more drops than we currently matched.
                var filteredMatches = _matches.Where(match => match.Data.ParticipantCount <= tiles.Count);

                foreach (var match in filteredMatches)
                {
                    foundMatch = match.IsMatch(tile, tiles, out participants);

                    if (foundMatch) 
                        matches.Add(participants);
                }
            });

            return !foundMatch ? null : matches;
        }

        private HashSet<Tile.Tile> FloodFill(Tile.Tile tile)
        {
            var tilesQueue = new Queue<Tile.Tile>();
            //used to check if tile is already in queue, I used a separate collection to effectively check in O(1)
            var tilesHashSet = new HashSet<Tile.Tile>();

            var directions = new List<Vector2Int>()
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };
            
            tilesQueue.Enqueue(tile);
            tilesHashSet.Add(tile);

            Tile.Tile currentTile = null;
            
            while (tilesQueue.Any())
            {
                currentTile = tilesQueue.Dequeue();

                foreach (var direction in directions)
                {
                    if (!_board.TileExists(currentTile.Data.Coordinates + direction, out var adjacentTile)) 
                        continue;
                    
                    if(tilesHashSet.Contains(adjacentTile))
                        continue;
                    
                    if(adjacentTile.IsEmpty() || tile.Data.CurrentDrop.Color != adjacentTile.Data.CurrentDrop.Color)
                        continue;
                    
                    tilesQueue.Enqueue(adjacentTile);
                    tilesHashSet.Add(adjacentTile);
                }
            }

            return tilesHashSet;
        }
    }
}