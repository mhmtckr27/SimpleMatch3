using System;
using System.Collections.Generic;
using SimpleMatch3.Board.Data;
using SimpleMatch3.Drop;
using UnityEngine;

namespace SimpleMatch3.BoardFactory
{
    public interface IBoardCreationStrategy
    {
        public Board.Board CreateBoard(BoardData boardData);
    }
}