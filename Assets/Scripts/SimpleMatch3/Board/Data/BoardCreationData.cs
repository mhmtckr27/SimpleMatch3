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
        public int minimumMatchAmount = 3;
        public int rowCount = 8;
        public int columnCount = 8;
        public Transform tilesParent;
        public Transform dropsParent;
        public List<Vector2Int> tilesToSkip = new();
        public List<Vector2Int> generatorTiles = new();
        public bool UsePreset;
        [SerializedDictionary(keyName:"Coordinate", valueName:"Color")]
        public SerializedDictionary<Vector2Int, DropColor> boardColors = new();
        
        [SerializedDictionary("Color", "Prefab")]
        public SerializedDictionary<DropColor, GameObject> dropPrefabs;
    }
}