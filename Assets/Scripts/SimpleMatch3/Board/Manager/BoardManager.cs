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
        
            var anyExplosions = false;
            var explodedTiles = new HashSet<Tile.Tile>();
            if (data.Task1.IsCompleted && data.Task1.Result != null)
            {
                ExplodeTiles(data.Task1.Result, data.SwipedTile).ForEach(t => explodedTiles.Add(t));
                anyExplosions = true;
            }
            if(data.Task2.IsCompleted && data.Task2.Result != null)
            {
                ExplodeTiles(data.Task2.Result, data.OtherTile).ForEach(t => explodedTiles.Add(t));
                anyExplosions = true;
            }
            
            if(anyExplosions)
            {
                data.SwipedTile.SetBusy(false);
                data.OtherTile.SetBusy(false);
                var allUpperTiles = _board.GetAllUpperTilesGroupedByColumns(explodedTiles);
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
                    await Task.Delay(500);
                
                var tilesToCheck = allUpperTiles.SelectMany(x => x).Concat(explodedTiles).Distinct();
                var matchTasks = new Dictionary<Tile.Tile, Task<List<MatchCoordinateOffsets>>>();
                
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

                var bestMatchTask = matchTasks.ElementAt(0);

                for(var i = 1; i < matchTasks.Count; i++)
                {
                    var currentTask = matchTasks.ElementAt(i);
                    
                    if(currentTask.Value.Result == null)
                        continue;
                    
                    if (bestMatchTask.Value.Result == null || currentTask.Value.Result.Count > bestMatchTask.Value.Result.Count)
                        bestMatchTask = currentTask;
                }

                //
                // var bestMatch = matchTasks.Where(t => t.Value.Result != null).Max(pair => pair.Value.Result.Count);
                // foreach (var matchTask in matchTasks)
                // {
                //     matchTask.Value?.Result?.ForEach(x => x.Offsets.ForEach(y => Debug.LogError(x + " : " + (y + matchTask.Key.Data.Coordinates))));
                // }
                //

                // foreach (var task in matchTasks)
                // {
                ;
                if(bestMatchTask.Value.Result == null)
                    return;
                ;
                await OnSwipeCompleted(new SwipeCompletedData()
                {
                    Task1 = bestMatchTask.Value,
                    Task2 = bestMatchTask.Value,
                    OtherTile = bestMatchTask.Key,
                    SwipedTile = bestMatchTask.Key,
                    SwipeDirection = data.SwipeDirection,
                    IsFromSwipe = false
                });

                await CheckConsecutiveExplosions(bestMatchTask.Value, bestMatchTask.Key);
                
                // await Task.Delay(30);
                // }

                
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

        private async Task CheckConsecutiveExplosions(Task<List<MatchCoordinateOffsets>> task, Tile.Tile tile)
        {
            
        }

        private List<Tile.Tile> ExplodeTiles(List<MatchCoordinateOffsets> coordinateOffsetsList, Tile.Tile tile)
        {
            var explodedTiles = new List<Tile.Tile>();
            foreach (var offset in coordinateOffsetsList.SelectMany(offsets => offsets.Offsets))
            {
                if (!_board.TileExists(tile.Data.Coordinates + offset, out var tileToExplode))
                    continue;

                tileToExplode.Explode();
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
        public Task<List<MatchCoordinateOffsets>> Task1;
        public Task<List<MatchCoordinateOffsets>> Task2;
        public bool IsFromSwipe;
    }
}