using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using SimpleMatch3.Generator;
using SimpleMatch3.Tile;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.BoardFactory
{
    public abstract class BoardCreationStrategyBase : IBoardCreationStrategy
    {
        protected readonly BoardCreationStrategyData Data;

        protected BoardCreationStrategyBase(BoardCreationStrategyData data)
        {
            Data = data;
        }
        
        public virtual Board.Board CreateBoard(BoardCreationData boardCreationData)
        {
            var board = Data.Instantiator.Instantiate<Board.Board>(new object[] {boardCreationData});
            var generators = new List<Generator.Generator>();
            
            for (var colIndex = 0; colIndex < boardCreationData.ColumnCount; colIndex++)
            {
                for (var rowIndex = 0; rowIndex < boardCreationData.RowCount; rowIndex++)
                {
                    var coords = new Vector2Int(colIndex, rowIndex);
                    
                    if(boardCreationData.TilesToSkip.Contains(coords))
                        continue;
                    
                    var isGeneratorTile = boardCreationData.GeneratorTiles.Contains(coords);
                    
                    Generator.Generator generator = null;
                    if (isGeneratorTile)
                        generator = CreateGenerator(coords, Vector3.zero);
                    
                    var tile = CreateTile(new TileData(coords, isGeneratorTile, generator));
                    
                    if (generator != null)
                        generator.Data.Position = tile.transform.position;
                    
                    var drop = CreateDrop(board, boardCreationData, tile);
                    
                    
                    drop.transform.position = tile.transform.position;
                    tile.SetDrop(drop);
                    board.AddTile(coords, tile);
                }
            }

            // foreach (var generator in generators)
            // {
            //     generator.Activate();
            // }
            
            return board;
        }

        private Generator.Generator CreateGenerator(Vector2Int coords, Vector3 position)
        {
            var generatorData = new GeneratorData()
            {
                Instantiator = Data.Instantiator,
                DropPrefabs = Data.DropPrefabs,
                DropsParent = Data.DropsParent,
                ColumnIndex = coords.x,
                Position = position,
                Coords = coords
            };
            
            return Data.Instantiator.Instantiate<Generator.Generator>(new object[] {generatorData});
        }

        private Tile.Tile CreateTile(TileData tileData)
        {
            var tile = Data.Instantiator.InstantiatePrefab(Data.TilePrefab, Data.TilesParent).GetComponent<Tile.Tile>();
            tile.name = $"Tile_{tileData.Coordinates}";
            tile.transform.localPosition = new Vector3(tileData.Coordinates.x, tileData.Coordinates.y);
            tile.Data = tileData;
            return tile;
        }

        protected abstract Drop.Drop CreateDrop(Board.Board board, BoardCreationData boardCreationData, Tile.Tile tile);
    }
    
    public class BoardCreationStrategyData
    {
        public IInstantiator Instantiator;
        public SignalBus SignalBus;
        public GameObject TilePrefab;
        public Transform TilesParent;
        public Transform DropsParent;
        public Dictionary<DropColor, GameObject> DropPrefabs;
    }
}