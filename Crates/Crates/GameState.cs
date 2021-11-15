using System;
using System.Collections.Generic;
using System.Text;

namespace Crates
{
    public class GameState
    {
        #region FIELDS
        private Cell[,] grid;
        private int crateRow;
        private int crateCol;
        private int guardRow;
        private int guardCol;
        private string outcomeClass;
        private List<GameState> leftChildren;
        private List<GameState> rightChildren;
        private bool printAllMoves = false;
        private bool printStashed = false;
        public bool impartial = true;
        private bool[,] gHit;
        private string lastToMove;
        #endregion

        #region CONSTRUCTOR
        public GameState(Cell[,] gridToCopy, int crateRow, int crateCol, int guardRow, int guardCol, string leftOrRight = "", bool[,] gHit = null)
        {
            grid = new Cell[gridToCopy.GetLength(0), gridToCopy.GetLength(1)];
            this.gHit = new bool[grid.GetLength(0), grid.GetLength(1)];
            for (int i = 0; i < gridToCopy.GetLength(0); i++)
            {
                for (int j = 0; j < gridToCopy.GetLength(1); j++)
                {
                    grid[i, j] = Cell.DeepCopy(gridToCopy[i, j]);
                    if (gHit == null) this.gHit[i, j] = false;
                    else this.gHit[i, j] = gHit[i, j];
                }
            }
            this.crateRow = crateRow;
            this.crateCol = crateCol;
            this.guardRow = guardRow;
            this.guardCol = guardCol;
            this.lastToMove = leftOrRight;
            outcomeClass = "";
            leftChildren = new List<GameState>();
            rightChildren = new List<GameState>();
        }
        #endregion

        #region GETTERS AND SETTERS
        public Cell[,] GetGrid()
        {
            return grid;
        }

        public void SetOutcomeClass(string s)
        {
            outcomeClass = s;
        }

        public string GetOutcomeClass()
        {
            return outcomeClass;
        }

        public List<GameState> GetLeftChildren()
        {
            return leftChildren;
        }

        public List<GameState> GetRightChildren()
        {
            return rightChildren;
        }

        public int CrateRow
        {
            get { return crateRow; }
            set { crateRow = value; }
        }
        public int CrateCol
        {
            get { return crateCol; }
            set { crateCol = value; }
        }
        public int GuardRow
        {
            get { return guardRow; }
            set { guardRow = value; }
        }
        public int GuardCol
        {
            get { return guardCol; }
            set { guardCol = value; }
        }
        public string LastToMove
        {
            get { return lastToMove; }
        }
        #endregion

        #region METHODS
        public void FindOutComeClass()
        {
            // Base case - outcome class has already been established in the creation of the game tree, and this branch has reached an end.
            if(outcomeClass != "")
            {
                return;
            }

            // Recursively solve for left children.
            foreach (GameState g in leftChildren)
            {
                g.FindOutComeClass();
            }
            // Recursively solve for right children.
            foreach (GameState g in rightChildren)
            {
                g.FindOutComeClass();
            }

            // Now that we have the solution to both right and left, we can figure out what outcome class this position is.
            bool someGLInLOrP = false;
            foreach (GameState g in leftChildren)
            {
                if (g.GetOutcomeClass() == "L" || g.GetOutcomeClass() == "P")
                {
                    someGLInLOrP = true;
                    break;
                }
            }
            bool someGRInROrP = false;
            foreach (GameState g in rightChildren)
            {
                if (g.GetOutcomeClass() == "R" || g.GetOutcomeClass() == "P")
                {
                    someGRInROrP = true;
                    break;
                }
            }
            if (someGLInLOrP && someGRInROrP) { 
                SetOutcomeClass("N");
                int hash = GetHashCode();
                if(!Program.outcomeLookUp.ContainsKey(hash)) Program.outcomeLookUp.Add(GetHashCode(), "N");
            }
            else if (!someGLInLOrP && someGRInROrP) 
            { 
                SetOutcomeClass("R");
                int hash = GetHashCode();
                if (!Program.outcomeLookUp.ContainsKey(hash)) Program.outcomeLookUp.Add(GetHashCode(), "R");
            }
            else if (someGLInLOrP && !someGRInROrP) 
            { 
                SetOutcomeClass("L");
                int hash = GetHashCode();
                if (!Program.outcomeLookUp.ContainsKey(hash)) Program.outcomeLookUp.Add(GetHashCode(), "L");
            }
            else if(!someGLInLOrP && !someGRInROrP) 
            { 
                SetOutcomeClass("P");
                int hash = GetHashCode();
                if (!Program.outcomeLookUp.ContainsKey(hash)) Program.outcomeLookUp.Add(GetHashCode(), "P");
            }
        }

        public void FindOutComeClassImpartial()
        {
            if (outcomeClass != "") return;

            foreach(GameState g in leftChildren)
            {
                g.FindOutComeClassImpartial();
            }

            bool someP = false;

            foreach(GameState g in leftChildren)
            {
                if(g.GetOutcomeClass() == "P")
                {
                    someP = true;
                    break;
                }
            }
            if (someP) SetOutcomeClass("N");
            else SetOutcomeClass("P");
        }

        public void GenerateChildren()
        {
            if (impartial)
            {
                GenerateChildrenImpartialCrates();
            }
            else
            {
                GenerateLeftChildrenCrates();
                GenerateRightChildrenCrates();
            }

            if (printAllMoves)
            {
                Console.WriteLine("Parent:");
                Print();
                Console.WriteLine("LEFT CHILDREN:");
                foreach (GameState g in leftChildren)
                {
                    g.Print();
                }

                Console.WriteLine("RIGHT CHILDREN:");
                foreach (GameState g in rightChildren)
                {
                    g.Print();
                }
            }
        }
        private void GenerateLeftChildrenCrates()
        {
            // We already marked this state as final, so do not generate any children.
            if (outcomeClass != "") return;

            // Check if we already know the outcome class of this particular board.
            int hash = GetHashCode();
            if (Program.outcomeLookUp.ContainsKey(hash))
            {
                SetOutcomeClass(Program.outcomeLookUp[hash]);
                return;
            }

            // Place conveyers where possible.
            for (int i = crateRow; i < grid.GetLength(0); i++)
            {
                for (int j = crateCol; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] != null && !grid[i, j].Trap && !grid[i, j].Crate && !grid[i, j].Guard && !grid[i, j].End && !grid[i, j].Conveyer)
                    {
                        // Place right conveyer
                        GameState rightConveyer = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Left");
                        rightConveyer.PlaceConveyer(i, j, Direction.Right);
                        leftChildren.Add(rightConveyer);

                        // Place down conveyer
                        GameState downConveyer = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Left");
                        downConveyer.PlaceConveyer(i, j, Direction.Down);
                        leftChildren.Add(downConveyer);
                    }
                }
            }
            // Push crate right.
            GameState pushRight = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Left");
            if (pushRight.PushCrate(Direction.Right))
            {
                if(pushRight.outcomeClass == "P")
                {
                    leftChildren = new List<GameState>();
                    leftChildren.Add(pushRight);
                    return;
                }
                leftChildren.Add(pushRight);
            }

            // Push crate down.
            GameState pushDown = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Left");
            if (pushDown.PushCrate(Direction.Down))
            {
                if (pushDown.outcomeClass == "P")
                {
                    leftChildren = new List<GameState>();
                    leftChildren.Add(pushDown);
                    return;
                }
                leftChildren.Add(pushDown);
            }
        }

        private void GenerateRightChildrenCrates()
        {
            // We already marked this state as final, so do not generate any children.
            if (outcomeClass != "") return;

            // Place a trap
            if (!grid[guardRow, guardCol].End && !grid[guardRow, guardCol].Conveyer && !grid[guardRow, guardCol].Trap)
            {
                GameState placeTrap = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Right", gHit);
                placeTrap.GetGrid()[guardRow, guardCol].Trap = true;
                rightChildren.Add(placeTrap);
            }

            // Move guard up, so long as the guard hasn't moved here yet.
            if (guardRow - 1 >= 0 && !gHit[guardRow - 1, guardCol])
            {
                Cell c = grid[guardRow - 1, guardCol];
                if (c != null && !c.End)
                {
                    // Copy the gamestate.
                    GameState moveUp = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Right", gHit);
                    // Move the guard.
                    moveUp.MoveGuard(Direction.Up);
                    rightChildren.Add(moveUp);
                }
            }
            // Move Guard down
            if (guardRow + 1 < grid.GetLength(0) && !gHit[guardRow + 1, guardCol])
            {
                Cell c = grid[guardRow + 1, guardCol];
                if (c != null && !c.End)
                {
                    // Copy the gamestate.
                    GameState moveDown = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Right", gHit);
                    // Move the guard.
                    moveDown.MoveGuard(Direction.Down);
                    rightChildren.Add(moveDown);
                }
            }
            // Move guard left
            if (guardCol - 1 >= 0 && !gHit[guardRow, guardCol - 1])
            {
                Cell c = grid[guardRow, guardCol - 1];
                if (c != null && !c.End)
                {
                    // Copy the gamestate.
                    GameState moveLeft = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Right", gHit);
                    // Move the guard.
                    moveLeft.MoveGuard(Direction.Left);
                    rightChildren.Add(moveLeft);
                }
            }
            // Move guard right
            if (guardCol + 1 < grid.GetLength(1) && !gHit[guardRow, guardCol + 1])
            {
                Cell c = grid[guardRow, guardCol + 1];
                if (c != null && !c.End)
                {
                    // copy the gamestate
                    GameState moveRight = new GameState(grid, crateRow, crateCol, guardRow, guardCol, "Right", gHit);
                    // Move the guard
                    moveRight.MoveGuard(Direction.Right);
                    rightChildren.Add(moveRight);
                }
            }

            if (rightChildren.Count == 0)
            {
                //Console.WriteLine("Uhh... so right didn't have anywhere to move.");
                SetOutcomeClass("L");
            }
        }

        public void GenerateChildrenImpartialCrates()
        {
            // We already marked this state as final, so do not generate any children.
            if (outcomeClass != "") return;

            // Check if we already know the outcome class of this particular board.
            int hash = GetHashCode();
            if (Program.outcomeLookUp.ContainsKey(hash))
            {
                if (printStashed)
                {
                    Console.WriteLine("Already know the outcome of this game is: " + outcomeClass);
                    Print();
                }
                SetOutcomeClass(Program.outcomeLookUp[hash]);
                return;
            }

            string childIsPlayerOf = lastToMove == "" || lastToMove == "Right" ? "Left" : "Right";

            // Place conveyers where possible.
            for (int i = crateRow; i < grid.GetLength(0); i++)
            {
                for (int j = crateCol; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] != null && !grid[i, j].Trap && !grid[i, j].Crate && !grid[i, j].Guard && !grid[i, j].End && !grid[i, j].Conveyer)
                    {
                        // Place right conveyer
                        GameState rightConveyer = new GameState(grid, crateRow, crateCol, guardRow, guardCol, childIsPlayerOf);
                        rightConveyer.PlaceConveyer(i, j, Direction.Right);
                        leftChildren.Add(rightConveyer);

                        // Place down conveyer
                        GameState downConveyer = new GameState(grid, crateRow, crateCol, guardRow, guardCol, childIsPlayerOf);
                        downConveyer.PlaceConveyer(i, j, Direction.Down);
                        leftChildren.Add(downConveyer);
                    }
                }
            }
            // Push crate right.
            GameState pushRight = new GameState(grid, crateRow, crateCol, guardRow, guardCol, childIsPlayerOf);
            if (pushRight.PushCrate(Direction.Right))
            {
                if (pushRight.outcomeClass == "P")
                {
                    rightChildren = new List<GameState>();
                    leftChildren.Add(pushRight);
                    return;
                }
                leftChildren.Add(pushRight);
            }

            // Push crate down.
            GameState pushDown = new GameState(grid, crateRow, crateCol, guardRow, guardCol, childIsPlayerOf);
            if (pushDown.PushCrate(Direction.Down))
            {
                if (pushDown.outcomeClass == "P")
                {
                    rightChildren = new List<GameState>();
                    leftChildren.Add(pushDown);
                    return;
                }
                leftChildren.Add(pushDown);
            }
        }
        #endregion

        #region UTILITY METHODS
        public void MoveGuard(Direction direction)
        {
            Cell moveFrom = grid[guardRow, guardCol];
            Cell moveTo = moveFrom;
            switch (direction){
                case Direction.Up:
                    {
                        moveTo = grid[guardRow - 1, guardCol];
                        // Update the guards position.
                        guardRow--;
                        break;
                    }
                case Direction.Down:
                    {
                        moveTo = grid[guardRow + 1, guardCol];
                        // Update the guards position.
                        guardRow++;
                        break;
                    }
                case Direction.Left:
                    {
                        moveTo = grid[guardRow, guardCol - 1];
                        // Update the guards position.
                        guardCol--;
                        break;
                    }
                case Direction.Right:
                    {
                        moveTo = grid[guardRow, guardCol + 1];
                        // Update the guards position.
                        guardCol++;
                        break;
                    }
            }

            // Remove the guard from it's previous position.
            moveFrom.Guard = false;
            // Move the guard to the new position.
            moveTo.Guard = true;
            moveTo.Empty = false;

            // Update where the guard has been.
            gHit[guardRow, guardCol] = true;

            // Mark the cell as empty if there is not a conveyer or trap.
            if (!moveFrom.Conveyer && !moveFrom.Trap)
            {
                moveFrom.Empty = true;
            }

            // The guard intersected the crate. So mark this as a R position (meaning right won).
            if (moveTo.Crate)
            {
                SetOutcomeClass("R");
            }
        }

        public void PlaceConveyer(int row, int col, Direction dir)
        {
            grid[row, col].Conveyer = true;
            grid[row, col].Direction = dir;
            grid[row, col].Empty = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns> True if the crate was pushed to a new location, false if it returns to where it began, or is stuck in an infinite loop</returns>
        public bool PushCrate(Direction dir)
        {
            bool sliding = true;
            bool hitConveyer = false;
            Direction travelDirection = dir;
            int currentRow = crateRow;
            int currentCol = crateCol;
            while (sliding)
            {
                int futureRow = currentRow;
                int futureCol = currentCol;
                switch (travelDirection)
                {
                    case Direction.Right:
                        {
                            futureCol++;
                            break;
                        }
                    case Direction.Down:
                        {
                            futureRow++;
                            break;
                        }                       
                }

                // The crate has hit a wall, stop moving
                if ((futureRow >= grid.GetLength(0) || futureCol >= grid.GetLength(1)) || grid[futureRow, futureCol] == null)
                {
                    sliding = false;
                }
                // Crate collides with the guard.
                else if (grid[futureRow, futureCol].Guard)
                {
                    grid[currentRow, currentCol].Crate = false;
                    if (!grid[currentRow, currentCol].Conveyer) { grid[currentRow, currentCol].Empty = true; }   // Crate moved out of this tile, and is now empty.
                    grid[futureRow, futureCol].Crate = true;
                    grid[futureRow, futureCol].Empty = false;
                    currentRow = futureRow;
                    currentCol = futureCol;
                    SetOutcomeClass("R");
                    sliding = false;
                }
                // Crate will hit a trap
                else if(grid[futureRow, futureCol].Trap)
                {
                    // move to new position, stop, and remove trap.
                    grid[currentRow, currentCol].Crate = false;
                    if(!grid[currentRow, currentCol].Conveyer) { grid[currentRow, currentCol].Empty = true; }   // Crate moved out of this tile, and is now empty.
                    grid[futureRow, futureCol].Crate = true;
                    grid[futureRow, futureCol].Empty = false;
                    grid[futureRow, futureCol].Trap = false;
                    currentRow = futureRow;
                    currentCol = futureCol;
                    sliding = false;
                }
                // Reaches the end zone
                else if (grid[futureRow, futureCol].End)
                {
                    grid[currentRow, currentCol].Crate = false;
                    if (!grid[currentRow, currentCol].Conveyer) { grid[currentRow, currentCol].Empty = true; }   // Crate moved out of this tile, and is now empty.
                    grid[futureRow, futureCol].Crate = true;
                    grid[futureRow, futureCol].Empty = false;
                    SetOutcomeClass("P");
                    currentRow = futureRow;
                    currentCol = futureCol;
                    sliding = false;
                }
                // Crate will move onto the conveyer and change direction
                else if (grid[futureRow, futureCol].Conveyer)
                {
                    grid[currentRow, currentCol].Crate = false;
                    if (!grid[currentRow, currentCol].Conveyer) { grid[currentRow, currentCol].Empty = true; }   // Crate moved out of this tile, and is now empty.
                    grid[futureRow, futureCol].Crate = true;
                    grid[futureRow, futureCol].Empty = false;
                    travelDirection = grid[futureRow, futureCol].Direction;                                     // Change direction.
                    currentRow = futureRow;
                    currentCol = futureCol;
                    hitConveyer = true;
                }
                // Crate will move into the un occupied cell.
                else if(grid[futureRow, futureCol].Empty)
                {
                    grid[currentRow, currentCol].Crate = false;
                    if (!grid[currentRow, currentCol].Conveyer) { grid[currentRow, currentCol].Empty = true; }   // Crate moved out of this tile, and is now empty.
                    grid[futureRow, futureCol].Crate = true;
                    grid[futureRow, futureCol].Empty = false;
                    currentRow = futureRow;
                    currentCol = futureCol;
                    if (!hitConveyer)
                    {
                        sliding = false;
                    }
                }
            }

            int hash = GetHashCode();
            // Outcome class has been computed (crate reached end zone or collided with a guard.
            if(!Program.outcomeLookUp.ContainsKey(hash) && outcomeClass != "")
            {
                Program.outcomeLookUp.Add(hash, outcomeClass);
            }
            // Outcome class for this state has already been computed, so set the outcome class.
            else if (Program.outcomeLookUp.ContainsKey(hash))
            {
                SetOutcomeClass(Program.outcomeLookUp[hash]);
                if (printStashed)
                {
                    Console.WriteLine("Already know the outcome of this game is: " + outcomeClass);
                    Print();
                }
            }

            // If the crate is in a different cell than it started in, return true.
            bool moved = currentRow != crateRow || currentCol != crateCol;
            crateRow = currentRow;
            crateCol = currentCol;
            return moved;
        }

        public void Print()
        {
            for(int i = 0; i < grid.GetLength(0); i++)
            {
                for(int j = 0; j < grid.GetLength(1); j++)
                {
                    Cell cell = grid[i, j];
                    char symbol = ' ';
                    if (cell == null) { symbol = 'X'; }
                    else if(cell.Crate) { symbol = 'C'; }
                    else if (cell.Guard) { symbol = 'G'; }
                    else if(cell.End) { symbol = 'E'; }
                    else if (cell.Empty) { symbol = '-'; }
                    else if (cell.Conveyer)
                    {
                        switch (cell.Direction){
                            case Direction.Up:
                                {
                                    symbol = (char)8593;
                                    break;
                                }
                            case Direction.Left:
                                {
                                    symbol = (char)8592;
                                    break;
                                }
                            case Direction.Right:
                                {
                                    symbol = (char)8594;
                                    break;
                                }
                            case Direction.Down:
                                {
                                    symbol = (char)8595;
                                    break;
                                }

                        }
                    }
                    else if (cell.Trap) { symbol = 'T'; }

                    Console.Write(symbol + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Yields a unique identifier per game state.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 0;
            int rightConveyer = 73;
            int downConveyer = 79;
            int crate = 83;
            int empty = 89;
            int offBoard = 97;
            int endZone = 101;
            int[,] boardPrimes = new int[10, 10]
            {
                { 419,   421, 431, 433, 439, 443, 449, 457, 461, 463 },
                { 547,   557, 563, 569, 571, 577, 587, 593, 599, 601},
                { 661,   673, 677, 683, 691, 701, 709, 719, 727, 733 },
                { 811,   821, 823, 827, 829, 839, 853, 857, 859, 863 },
                { 947,   953, 967, 971, 977, 983, 991, 997, 1009, 1013},
                { 1087,  1091,    1093,    1097,    1103,    1109,    1117,    1123,    1129,    1151 },
                { 1229,  1231,    1237,    1249,    1259,    1277,    1279,    1283,   1289,    1291},
                { 1381,  1399,    1409,    1423,    1427,    1429,    1433,    1439,    1447,    1451},
                { 1523,  1531,    1543,    1549,    1553,    1559,    1567,    1571,    1579,    1583},
                { 1663,  1667,    1669,    1693,    1697,    1699,    1709,    1721,    1723,    1733}
            };

            for (int i = crateRow; i < grid.GetLength(0); i++)
            {
                for(int j = crateCol; j < grid.GetLength(1); j++)
                {
                    int relRow = i - crateRow;
                    int relCol = j - crateCol;
                    if (grid[i, j] == null) hash += boardPrimes[relRow, relCol] * offBoard;
                    else if (grid[i, j].Crate) hash += boardPrimes[relRow, relCol] * crate;
                    else if (grid[i, j].End) hash += boardPrimes[relRow, relCol] * endZone;
                    else if (grid[i, j].Conveyer && grid[i, j].Direction == Direction.Right) hash += boardPrimes[relRow, relCol] * rightConveyer;
                    else if (grid[i, j].Conveyer && grid[i, j].Direction == Direction.Down) hash += boardPrimes[relRow, relCol] * downConveyer;
                    else if (grid[i, j].Empty) hash += boardPrimes[relRow, relCol] * empty;
                }
            }
            return hash;
        }
        #endregion

        #region OLD RULES
        //private void GenerateLeftChildrenIce()
        //{
        //    // Can think of rotation as corners of a bounding square.
        //    for (int squareLength = 1; row >= squareLength || col > +squareLength || grid.GetLength(0) - row >= squareLength || grid.GetLength(1) - col >= squareLength; squareLength++)
        //    {
        //        int newRow, newCol;
        //        // Crate is in the bottom right of the square, and the rotation fits in the board.
        //        if (row - squareLength >= 0 && col - squareLength >= 0)
        //        {
        //            newRow = row - squareLength;
        //            newCol = col - squareLength;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row - squareLength;
        //            newCol = col;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //        }
        //        // Crate is in the bottom left of the square, and the rotation fits in the board.
        //        if (row - squareLength >= 0 && col + squareLength < grid.GetLength(1))
        //        {
        //            newRow = row - squareLength;
        //            newCol = col + squareLength;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row - squareLength;
        //            newCol = col;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //        }
        //        // Crate is in the top right of the square, and the rotation fits in the board.
        //        if (row + squareLength < grid.GetLength(0) && col - squareLength >= 0)
        //        {
        //            newRow = row + squareLength;
        //            newCol = col - squareLength;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row + squareLength;
        //            newCol = col;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //        }
        //        // Crate is in the top left of the square, and the rotation fits in the board.
        //        if (row + squareLength < grid.GetLength(0) && col + squareLength < grid.GetLength(1))
        //        {
        //            newRow = row + squareLength;
        //            newCol = col + squareLength;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //            newRow = row + squareLength;
        //            newCol = col;
        //            if (!grid[newRow, newCol])
        //            {
        //                GameState newLeftState = new GameState(grid, newRow, newCol);
        //                leftChildren.Add(newLeftState);
        //            }

        //        }
        //    }
        //}
        //private void GenerateLeftChildrenSimple()
        //{
        //    if (row - 1 >= 0 && col - 1 >= 0 && !grid[row - 1, col - 1])
        //    {
        //        GameState newLeftState = new GameState(grid, row - 1, col - 1);
        //        leftChildren.Add(newLeftState);
        //    }
        //    else if (row - 1 >= 0 && col + 1 < grid.GetLength(1) && !grid[row - 1, col + 1])
        //    {
        //        GameState newLeftState = new GameState(grid, row - 1, col + 1);
        //        leftChildren.Add(newLeftState);
        //    }
        //    else if (row + 1 < grid.GetLength(0) && col - 1 >= 0 && !grid[row + 1, col - 1])
        //    {
        //        GameState newLeftState = new GameState(grid, row + 1, col - 1);
        //        leftChildren.Add(newLeftState);
        //    }
        //    else if (row + 1 < grid.GetLength(0) && col + 1 < grid.GetLength(1) && !grid[row + 1, col + 1])
        //    {
        //        GameState newLeftState = new GameState(grid, row + 1, col + 1);
        //        leftChildren.Add(newLeftState);
        //    }
        //}

        //private void GenerateRightChildrenIce()
        //{
        //    // All valid positions to the left of the square.
        //    for (int j = col - 1; j >= 0 && !grid[row, j]; j--)
        //    {
        //        GameState newRightState = new GameState(grid, row, j);
        //        rightChildren.Add(newRightState);
        //    }
        //    // All valid positions to the right of the square.
        //    for (int j = col + 1; j < grid.GetLength(1) && !grid[row, j]; j++)
        //    {
        //        GameState newRightState = new GameState(grid, row, j);
        //        rightChildren.Add(newRightState);
        //    }
        //    // All valid positions above the square.
        //    for (int i = row - 1; i > 0 && !grid[i, col]; i--)
        //    {
        //        GameState newRightState = new GameState(grid, i, col);
        //        rightChildren.Add(newRightState);
        //    }
        //    // All valid positions below the square.
        //    for (int i = row + 1; i < grid.GetLength(0) && !grid[i, col]; i++)
        //    {
        //        GameState newRightState = new GameState(grid, i, col);
        //        rightChildren.Add(newRightState);
        //    }
        //}

        //public void GenerateRightChildrenSimple()
        //{
        //    if (row - 1 >= 0 && !grid[row - 1, col])
        //    {
        //        GameState newRightState = new GameState(grid, row - 1, col);
        //        rightChildren.Add(newRightState);
        //    }
        //    else if (row + 1 < grid.GetLength(0) && !grid[row + 1, col])
        //    {
        //        GameState newRightState = new GameState(grid, row + 1, col);
        //        rightChildren.Add(newRightState);
        //    }
        //    if (col - 1 >= 0 && !grid[row, col - 1])
        //    {
        //        GameState newRightState = new GameState(grid, row, col - 1);
        //        rightChildren.Add(newRightState);
        //    }
        //    if (col + 1 < grid.GetLength(1) && !grid[row, col + 1])
        //    {
        //        GameState newRightState = new GameState(grid, row, col + 1);
        //        rightChildren.Add(newRightState);
        //    }
        //}
        #endregion
    }
}
