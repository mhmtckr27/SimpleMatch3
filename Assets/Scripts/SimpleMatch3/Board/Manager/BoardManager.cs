using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleMatch3.Board.Data;
using SimpleMatch3.BoardCreationStrategy;
using SimpleMatch3.BoardFactory;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Gravity;
using SimpleMatch3.Matching.Data;
using SimpleMatch3.Matching.Matches;
using SimpleMatch3.Matching.MatchProcessor;
using SimpleMatch3.Pool;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Board.Manager
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject boardMask;
        [field: SerializeField] public BoardCreationData BoardCreationData { get; private set; }

        private IInstantiator _instantiator;
        private SignalBus _signalBus;

        private IMatchProcessor _matchProcessor;
        private Board _board;
        private GravityProcessor _gravityProcessor;
        private Camera _mainCamera;

        [Inject]
        private void Construct(IInstantiator instantiator, SignalBus signalBus, Camera mainCamera, DropPools dropPools)
        {
            _instantiator = instantiator;
            _signalBus = signalBus;
            _mainCamera = mainCamera;
            
            var strategyData = new BoardCreationStrategyData()
            {
                Instantiator = _instantiator,
                SignalBus = _signalBus,
                DropPools = dropPools,
                DropsParent = BoardCreationData.dropsParent,
                TilePrefab = tilePrefab,
                TilesParent = BoardCreationData.tilesParent
            };
            
            //TODO: Expose to editor (to use which strategy.)
            IBoardCreationStrategy boardCreationStrategy = BoardCreationData.UsePreset
                ? new PresetBoardCreationStrategy(strategyData)
                : new RandomBoardCreationStrategy(strategyData);
            
            _board = boardCreationStrategy.CreateBoard(BoardCreationData);
            
            _matchProcessor = _instantiator.Instantiate<MatchProcessor>(new []{ _board });
            _gravityProcessor = _instantiator.Instantiate<GravityProcessor>(new []{ _board });
            
            _signalBus.Subscribe<ISwiped.OnSwiped>(OnSwiped);
        }
        
        private void Awake()
        {
            AdjustBoardPosition();
            FitBoardInCamera();
            ScaleBoardMask();
        }

        private void ScaleBoardMask()
        {
            boardMask.transform.localScale = new Vector3(BoardCreationData.columnCount, BoardCreationData.rowCount, 1);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<ISwiped.OnSwiped>(OnSwiped);
        }

        private void AdjustBoardPosition()
        {
            var tileBounds = tilePrefab.GetComponent<SpriteRenderer>().bounds;
            var tileSize = tileBounds.extents.x;
            Tile.Tile.TileSize = tileSize;
            transform.position = new Vector3(-(float) BoardCreationData.columnCount / 2 + tileSize,
                -(float) BoardCreationData.rowCount / 2 + tileSize);
        }

        private void FitBoardInCamera()
        {
            float w = Tile.Tile.TileSize * 2 * BoardCreationData.columnCount;
            float h = Tile.Tile.TileSize * 2 * BoardCreationData.rowCount;
            float x = w * 0.5f - 0.5f;
            float y = h * 0.5f - 0.5f;

            _mainCamera.orthographicSize = ((w > h * _mainCamera.aspect)
                ? w / _mainCamera.pixelWidth * _mainCamera.pixelHeight
                : h) / 2 + 1;
        }

        /// <summary>
        /// Called when swipe detected.
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// Waits for given list of tasks.
        /// </summary>
        /// <param name="tasks"></param>
        private async Task WaitTasks(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                await task;
            }
        }
        
        /// <summary>
        /// Called when swipe animation finished.
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// Processes gravity for exploded tiles and upper tiles of exploded tiles.
        /// </summary>
        /// <param name="allUpperTiles"></param>
        /// <param name="explodedTiles"></param>
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

            while (!_gravityProcessor.AreDropsStable(drops))
                await Task.Delay(10);
        }

        /// <summary>
        /// Called after explosions that are produced from user input (swipe). First waits for gravity task to become stable.
        /// Then searches for any matches that fallen drops produce. We don't need to
        /// check the columns other than of explodedTiles' columns for better performance.
        /// </summary>
        /// <param name="explodedTiles"></param>
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

        /// <summary>
        /// Checks given matching tasks, and calls ExplodeTiles with actual matches.
        /// Discards matching tasks that produce no explosions.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Explodes tiles with given coordinates.
        /// async because we need to wait drops to become stable (Finish its squash and stretch anim, etc.)
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private async Task<List<Tile.Tile>> ExplodeTiles(List<Vector2Int> coordinates)
        {
            var explodedTiles = new List<Tile.Tile>();
            var explodeTasks = new List<Task>();
            foreach (var coord in coordinates.Distinct())
            {
                if (!_board.TileExists(coord, out var tileToExplode))
                    continue;

                explodeTasks.Add(tileToExplode.Explode());
                explodedTiles.Add(tileToExplode);
            }

            foreach (var explodeTask in explodeTasks)
            {
                await explodeTask;
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