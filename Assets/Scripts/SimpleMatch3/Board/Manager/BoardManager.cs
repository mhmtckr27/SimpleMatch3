using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using SimpleMatch3.Board.Data;
using SimpleMatch3.BoardCreationStrategy;
using SimpleMatch3.BoardFactory;
using SimpleMatch3.Drop;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Gravity;
using SimpleMatch3.Matching.Data;
using SimpleMatch3.Matching.Matches;
using SimpleMatch3.Matching.MatchProcessor;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Board.Manager
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private BoardCreationData boardCreationData;

        [SerializedDictionary("Color", "Prefab")]
        public SerializedDictionary<DropColor, GameObject> dropPrefabs;

        private IInstantiator _instantiator;
        private SignalBus _signalBus;

        private IMatchProcessor _matchProcessor;
        private Board _board;
        private GravityProcessor _gravityProcessor;
        private Camera _mainCamera;

        [Inject]
        private void Construct(IInstantiator instantiator, SignalBus signalBus, Camera mainCamera)
        {
            _instantiator = instantiator;
            _signalBus = signalBus;
            _mainCamera = mainCamera;

            var tilesParent = CreateTilesParent();
            var dropsParent = CreateDropsParent();
            
            var strategyData = new BoardCreationStrategyData()
            {
                Instantiator = _instantiator,
                SignalBus = _signalBus,
                DropPrefabs = dropPrefabs,
                DropsParent = dropsParent,
                TilePrefab = tilePrefab,
                TilesParent = tilesParent
            };
            
            IBoardCreationStrategy boardCreationStrategy = new RandomBoardCreationStrategy(strategyData);
            _board = boardCreationStrategy.CreateBoard(boardCreationData);
            
            _matchProcessor = _instantiator.Instantiate<MatchProcessor>(new []{ _board });
            _gravityProcessor = _instantiator.Instantiate<GravityProcessor>(new []{ _board });
            
            _signalBus.Subscribe<ISwiped.OnSwiped>(OnSwiped);
        }
        
        private void Awake()
        {
            AdjustBoardPosition();
            FitBoardInCamera();
        }

        private Transform CreateDropsParent()
        {
            var dropsParent = new GameObject("Drops").transform;
            dropsParent.SetParent(transform, false);
            return dropsParent;
        }

        private Transform CreateTilesParent()
        {
            var tilesParent = new GameObject("Tiles").transform;
            tilesParent.SetParent(transform, false);
            return tilesParent;
        }

        private void AdjustBoardPosition()
        {
            var tileBounds = tilePrefab.GetComponent<SpriteRenderer>().bounds;
            var tileSize = tileBounds.extents.x;
            Tile.Tile.TileSize = tileSize;
            transform.position = new Vector3(-(float) boardCreationData.ColumnCount / 2 + tileSize,
                -(float) boardCreationData.RowCount / 2 + tileSize);
        }

        private void FitBoardInCamera()
        {
            _mainCamera.orthographicSize = boardCreationData.ColumnCount;
        }

        private void OnSwiped(ISwiped.OnSwiped data)
        {
            if(!_board.TileExists(data.InputDownTileCoords, out var swipedTile))
                return;
            
            if(swipedTile.IsBusy)
                return;
            
            if(swipedTile.IsEmpty())
                return;

            if (!_board.TileExists(data.InputDownTileCoords + data.SwipeDirection, out var otherTile) ||
                otherTile.IsEmpty()) 
            {
                swipedTile.PlaySwipeNotAllowedAnim(data.SwipeDirection);
                return;
            }        
            
            if(otherTile.IsBusy)
                return;

            var tempDrop = swipedTile.SetDrop(otherTile.Data.CurrentDrop);
            otherTile.SetDrop(tempDrop);
            
            var task1 = _matchProcessor.ProcessMatches(swipedTile);
            var task2 = _matchProcessor.ProcessMatches(otherTile);

            var swipeCompletedData = new SwipeCompletedData()
            {
                Task1 = task1,
                Task2 = task2,
                SwipeDirection = data.SwipeDirection,
                SwipedTile = swipedTile,
                OtherTile = otherTile,
                IsFromSwipe = true
            };

            swipedTile.PlaySwipeAnim(swipeCompletedData.SwipeDirection, otherTile, null);
            otherTile.PlaySwipeAnim(-swipeCompletedData.SwipeDirection, swipedTile, () => OnSwipeCompleted(swipeCompletedData));
        }

        private async Task WaitTasks(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                await task;
            }
        }
        
        private async Task OnSwipeCompleted(SwipeCompletedData data)
        {
            await WaitTasks(new List<Task>()
            {
                data.Task1,
                data.Task2
            });

            
            var result1 = CheckAndExplodeTiles(new Dictionary<Tile.Tile, Task<List<(IMatch, MatchCoordinateOffsets)>>>()
            {
                {
                    data.SwipedTile, data.Task1
                },
                {
                    data.OtherTile, data.Task2
                },
                
                
            });

            await result1;
            
            var anyExplosions = result1.Result.anyExplosions;
            var explodedTiles = result1.Result.explodedTiles;

            if(anyExplosions)
            {
                data.SwipedTile.SetBusy(false);
                data.OtherTile.SetBusy(false);

                await CheckConsecutiveExplosions(explodedTiles);

                return;
            }
        
            if(!data.IsFromSwipe)
                return;
            
            //if both tasks failed, e.g. there is no explosion, move drops to their original position.
            var tempDrop = data.SwipedTile.SetDrop(data.OtherTile.Data.CurrentDrop);
            data.OtherTile.SetDrop(tempDrop);
        
            data.SwipedTile.PlaySwipeAnim(data.SwipeDirection, data.OtherTile, () => data.SwipedTile.SetBusy(false));
            data.OtherTile.PlaySwipeAnim(-data.SwipeDirection, data.SwipedTile, () => data.OtherTile.SetBusy(false));
        }

        private async Task ProcessGravity(List<List<Tile.Tile>> allUpperTiles, HashSet<Tile.Tile> explodedTiles)
        {
            if(explodedTiles == null)
                return;
            var drops = new List<Drop.Drop>();
            foreach (var upperTiles in allUpperTiles)
            {
                var generator = upperTiles.FirstOrDefault(t => t.Data.IsGeneratorTile)?.Data.Generator;
                drops = upperTiles.Where(t => !t.IsEmpty()).Select(t => t.Data.CurrentDrop).ToList();
                if (generator != null)
                {
                    var generateCount = explodedTiles.Distinct().Count(x => x.Data.Coordinates.x == generator.Data.ColumnIndex);
                    for (int i = 0; i < generateCount; i++)
                    {
                        var drop = generator.GenerateDrop(generator.Data.Position + Vector3.up * Tile.Tile.TileSize * (i + 1) * 2);
                        drop.CurrentTileCoords = generator.Data.Coords;
                        drops.Add(drop);
                    }
                }

                StartCoroutine(_gravityProcessor.ProcessGravityForDrops(drops));
            }

            while (!_board.AreDropsStable(drops))
                await Task.Delay(10);
        }

        private async Task CheckConsecutiveExplosions(HashSet<Tile.Tile> explodedTiles)
        {
            var allUpperTiles = _board.GetAllUpperTilesGroupedByColumns(explodedTiles);
            await ProcessGravity(allUpperTiles, explodedTiles);
            var tilesToCheck = allUpperTiles.SelectMany(x => x).Concat(explodedTiles).Distinct();
            var matchTasks = new Dictionary<Tile.Tile, Task<List<(IMatch match, MatchCoordinateOffsets)>>>();
                
            foreach (var tile in tilesToCheck)
            {
                matchTasks.Add(tile, _matchProcessor.ProcessMatches(tile));
            }
                
            if(matchTasks.Count == 0)
                return;

            foreach (var task in matchTasks)
            {
                await task.Value;
            }

            matchTasks = matchTasks.Where(t => t.Value.Result != null).
            ToDictionary(t => t.Key, t => t.Value);
            
            foreach (var matchTask in matchTasks)
            {
                await matchTask.Value;
            }
            
            var result = CheckAndExplodeTiles(matchTasks);
            await result;
            if (result.Result.anyExplosions)
            {
                await CheckConsecutiveExplosions(result.Result.explodedTiles);
                await Task.Delay(10);
            }
        }

        private async Task<(bool anyExplosions, HashSet<Tile.Tile> explodedTiles)> CheckAndExplodeTiles(
            Dictionary<Tile.Tile, Task<List<(IMatch match, MatchCoordinateOffsets offsets)>>> tasks)
        {
            var explodedTiles = new HashSet<Tile.Tile>();
            var coordinates = new List<Vector2Int>();

            foreach (var task in tasks)
            {
                if (!task.Value.IsCompleted || task.Value.Result == null) 
                    continue;

                coordinates.AddRange(task.Value.Result.SelectMany(x => x.offsets.Offsets)
                    .Select(x => x + task.Key.Data.Coordinates));
            }

            if (!coordinates.Any()) 
                return (false, new HashSet<Tile.Tile>());
            
            //
            // while (!_board.AreDropsStable(drops))
            //     await Task.Delay(500);

            var explodeTask = ExplodeTiles(coordinates);
            await explodeTask;
            explodeTask.Result.ForEach(t => explodedTiles.Add(t));

            return (true, explodedTiles);
        }

        private async Task<List<Tile.Tile>> ExplodeTiles(List<Vector2Int> coordinates)
        {
            var explodedTiles = new List<Tile.Tile>();
            foreach (var coord in coordinates.Distinct())
            {
                if (!_board.TileExists(coord, out var tileToExplode))
                    continue;

                await tileToExplode.Explode();
                tileToExplode.SetDrop(null);
                explodedTiles.Add(tileToExplode);
            }

            return explodedTiles;
        }
    }
    
    public class SwipeCompletedData
    {
        public Vector2Int SwipeDirection;
        public Tile.Tile SwipedTile;
        public Tile.Tile OtherTile;
        public Task<List<(IMatch, MatchCoordinateOffsets)>> Task1;
        public Task<List<(IMatch, MatchCoordinateOffsets)>> Task2;
        public bool IsFromSwipe;
    }
}