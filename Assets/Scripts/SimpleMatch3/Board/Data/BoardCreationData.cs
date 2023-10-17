using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using SimpleMatch3.Drop;
using UnityEngine;

namespace SimpleMatch3.Board.Data
{

    [Serializable]
    public class BoardCreationData
    {
        public int MinimumMatchAmount = 3;
        public int RowCount = 8;
        public int ColumnCount = 8;
        public List<Vector2Int> TilesToSkip = new();
        public List<Vector2Int> GeneratorTiles = new();
        [SerializedDictionary(keyName:"Coordinate", valueName:"Color")]
        public SerializedDictionary<Vector2Int, DropColor> BoardColors = new();
    }
}