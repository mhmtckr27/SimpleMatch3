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

        [Inject]
        private void Construct(IInstantiator instantiator, SignalBus signalBus)
        {
            _instantiator = instantiator;
            _signalBus = signalBus;

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
            Camera.main.orthographicSize = boardCreationData.ColumnCount;
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
                OtherTile = otherTile,
                SwipeDirection = data.SwipeDirection,
                SwipedTile = swipedTile
            };

            var swipeCompletedData2 = new SwipeCompletedData2()
            {
                Task = task1,
                SwipeDirection = data.SwipeDirection,
                SwipedTile = swipedTile,
                OtherTile = otherTile,
                DropCache = otherTile.Data.CurrentDrop
            };

            var swipeCompletedData3 = new SwipeCompletedData2()
            {
                Task = task2,
                SwipeDirection = -data.SwipeDirection,
                SwipedTile = otherTile,
                OtherTile = swipedTile,
                DropCache = swipedTile.Data.CurrentDrop
            };

            async void OnCompleteSwipe(SwipeCompletedData2 daata)
            {
                var task = CheckMatch(daata);
                await task;
                
                if(task.Result)
                    return;

                //if both tasks failed, e.g. there is no explosion, move drop to its original position.
                daata.SwipedTile.SetDrop(daata.DropCache);
                daata.SwipedTile.PlaySwipeAnim(daata.SwipeDirection, daata.OtherTile, () => daata.SwipedTile.SetBusy(false));
            }

            swipedTile.PlaySwipeAnim(swipeCompletedData2.SwipeDirection, otherTile, (() => OnCompleteSwipe(swipeCompletedData2)));
            otherTile.PlaySwipeAnim(swipeCompletedData3.SwipeDirection, swipedTile, () => OnCompleteSwipe(swipeCompletedData3));
        }

        private async Task<bool> CheckMatch(SwipeCompletedData2 data)
        {
            await data.Task;

            var anyExplosions = false;
            var explodedTiles = new List<Tile.Tile>();
            if (data.Task.IsCompleted && data.Task.Result != null)
            {
                explodedTiles.AddRange(ExplodeTiles(data.Task.Result, data.SwipedTile));
                anyExplosions = true;
            }
            
            if(anyExplosions)
            {
                data.SwipedTile.SetBusy(false);
                
                var allUpperTiles = _board.GetAllUpperTilesGroupedByColumns(explodedTiles);
                var gravityTasks = new List<Task>();
                foreach (var upperTiles in allUpperTiles)
                {
                    gravityTasks.Add(_gravityProcessor.ProcessGravityFor(upperTiles));
                }

                Debug.LogError("BURADA 1");
                foreach (var gravityTask in gravityTasks)
                {
                    await gravityTask;
                }
                Debug.LogError("BURADA 2");

                // var tilesToCheck = allUpperTiles.SelectMany(x => x).Concat(explodedTiles).Distinct();
                // var matchTasks = new List<Task>();
                //
                // foreach (var tile in tilesToCheck)
                // {
                //     matchTasks.Add(_matchProcessor.ProcessMatches(tile));
                // }
            }

            return anyExplosions;
            //
            // //if both tasks failed, e.g. there is no explosion, move drops to their original position.
            // var tempDrop = data.SwipedTile.SetDrop(data.OtherTile.Data.CurrentDrop);
            // data.OtherTile.SetDrop(tempDrop);
            //
            // data.SwipedTile.PlaySwipeAnim(data.SwipeDirection, data.OtherTile, () => data.SwipedTile.SetBusy(false));
            // data.OtherTile.PlaySwipeAnim(-data.SwipeDirection, data.SwipedTile, () => data.OtherTile.SetBusy(false));
        }
        
        private async void OnSwipeCompleted(SwipeCompletedData data)
        {
            await data.Task1;
            await data.Task2;

            var anyExplosions = false;
            var explodedTiles = new List<Tile.Tile>();
            if (data.Task1.IsCompleted && data.Task1.Result != null)
            {
                explodedTiles.AddRange(ExplodeTiles(data.Task1.Result, data.SwipedTile));
                anyExplosions = true;
            }
            if(data.Task2.IsCompleted && data.Task2.Result != null)
            {
                explodedTiles.AddRange(ExplodeTiles(data.Task2.Result, data.OtherTile));
                anyExplosions = true;
            }
            
            if(anyExplosions)
            {
                data.SwipedTile.SetBusy(false);
                data.OtherTile.SetBusy(false);
                var allUpperTiles = _board.GetAllUpperTilesGroupedByColumns(explodedTiles);
                var gravityTasks = new List<Task>();
                foreach (var upperTiles in allUpperTiles)
                {
                    gravityTasks.Add(_gravityProcessor.ProcessGravityFor(upperTiles));
                }

                Debug.LogError("BURADA 1");
                foreach (var gravityTask in gravityTasks)
                {
                    await gravityTask;
                }
                Debug.LogError("BURADA 2");

                var tilesToCheck = allUpperTiles.SelectMany(x => x).Concat(explodedTiles).Distinct();
                var matchTasks = new List<Task>();
                
                foreach (var tile in tilesToCheck)
                {
                    matchTasks.Add(_matchProcessor.ProcessMatches(tile));
                }
                
                
                
                return;
            }

            //if both tasks failed, e.g. there is no explosion, move drops to their original position.
            var tempDrop = data.SwipedTile.SetDrop(data.OtherTile.Data.CurrentDrop);
            data.OtherTile.SetDrop(tempDrop);

            data.SwipedTile.PlaySwipeAnim(data.SwipeDirection, data.OtherTile, () => data.SwipedTile.SetBusy(false));
            data.OtherTile.PlaySwipeAnim(-data.SwipeDirection, data.SwipedTile, () => data.OtherTile.SetBusy(false));
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
    }
    public class SwipeCompletedData2
    {
        public Vector2Int SwipeDirection;
        public Tile.Tile SwipedTile;
        public Tile.Tile OtherTile;
        public Task<List<MatchCoordinateOffsets>> Task;
        public Drop.Drop DropCache;
    }
}