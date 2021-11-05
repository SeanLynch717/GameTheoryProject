using System;
using System.Collections.Generic;

namespace Crates
{
    class Program
    {
        static bool[,] grid;
        static void Main(string[] args)
        {
            grid = new bool[3,4];
            GameState root = new GameState(grid, 0, 0);
            Queue<GameState> queue = new Queue<GameState>();
            queue.Enqueue(root);

            GameState current;
            int count = 0;
            // Generate the entire game tree.
            while (queue.Count > 0)
            {
                count++;
                // Get the next element from the queue.
                current = queue.Dequeue();
                // Console.WriteLine(current.GetRow() + "  " + current.GetCol());
                // Generate all children of current.
                current.GenerateChildren();

                // Enqueue each child of current
                foreach(GameState g in current.GetLeftChildren())
                {
                    queue.Enqueue(g);
                }
                foreach(GameState g in current.GetRightChildren())
                {
                    queue.Enqueue(g);
                }
            }
            root.FindOutComeClass();
            Console.WriteLine(count);
            Console.WriteLine("Outcome class: " + root.GetOutcomeClass());    
        }
    }


    public class GameState
    {
        private bool[,] grid;
        private int row;
        private int col;
        private string outcomeClass;
        private List<GameState> leftChildren;
        private List<GameState> rightChildren;

        public GameState(bool[,] gridToCopy, int row, int col)
        {
            grid = new bool[gridToCopy.GetLength(0), gridToCopy.GetLength(1)];
            for(int i = 0; i < gridToCopy.GetLength(0); i++)
            {
                for(int j = 0; j < gridToCopy.GetLength(1); j++)
                {
                    grid[i, j] = gridToCopy[i, j];
                }
            }
            this.row = row;
            this.col = col;
            grid[row, col] = true;
            outcomeClass = "";
            leftChildren = new List<GameState>();
            rightChildren = new List<GameState>();
        }
        public void FindOutComeClass()
        {
            // Base case - no children so we know it's a p position.
            if (leftChildren.Count == 0 && rightChildren.Count == 0)
            {
                SetOutcomeClass("P");
                return;
            }

            // Recursively solve for left children.
            foreach(GameState g in leftChildren)
            {
                g.FindOutComeClass();
            }
            // Recursively solve for right children.
            foreach(GameState g in rightChildren)
            {
                g.FindOutComeClass();
            }

            // Now that we have the solution to both right and left, we can figure out what outcome class this position is.
            bool someGLInLOrP = false;
            foreach(GameState g in leftChildren)
            {
                if(g.GetOutcomeClass() == "L" || g.GetOutcomeClass() == "P")
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
            if(someGLInLOrP && someGRInROrP) { SetOutcomeClass("N"); }
            else if(!someGLInLOrP && someGRInROrP) { SetOutcomeClass("R"); }
            else if(someGLInLOrP && !someGRInROrP) { SetOutcomeClass("L"); }
            else { SetOutcomeClass("P"); }
        }

        public void GenerateChildren()
        {
            // Generate left children.
            GenerateLeftChildren();

            // Generte right children.
            GenerateRightChildren();
        }

        private void GenerateLeftChildren()
        {
            // Can think of rotation as corners of a bounding square.
            for (int squareLength = 1; row >= squareLength || col >+ squareLength || grid.GetLength(0) - row >= squareLength || grid.GetLength(1) - col >= squareLength; squareLength++)
            {
                int newRow, newCol;
                // Crate is in the bottom right of the square, and the rotation fits in the board.
                if (row - squareLength >= 0 && col - squareLength >= 0)
                {
                    newRow = row - squareLength;
                    newCol = col - squareLength;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);             
                    }

                    newRow = row;
                    if(!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row - squareLength;
                    newCol = col;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                }
                // Crate is in the bottom left of the square, and the rotation fits in the board.
                if (row - squareLength >= 0 && col + squareLength < grid.GetLength(1))
                {
                    newRow = row - squareLength;
                    newCol = col + squareLength;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row - squareLength;
                    newCol = col;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                }
                // Crate is in the top right of the square, and the rotation fits in the board.
                if (row + squareLength < grid.GetLength(0) && col - squareLength >= 0)
                {
                    newRow = row + squareLength;
                    newCol = col - squareLength;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row + squareLength;
                    newCol = col;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                }
                // Crate is in the top left of the square, and the rotation fits in the board.
                if (row + squareLength < grid.GetLength(0) && col + squareLength < grid.GetLength(1))
                {
                    newRow = row + squareLength;
                    newCol = col + squareLength;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                    newRow = row + squareLength;
                    newCol = col;
                    if (!grid[newRow, newCol])
                    {
                        GameState newLeftState = new GameState(grid, newRow, newCol);
                        leftChildren.Add(newLeftState);
                    }

                }
            }
        }
        private void GenerateLeftChildrenSimple()
        {
            if(row - 1 >= 0 && col - 1 >= 0 && !grid[row - 1, col - 1])
            {
                GameState newLeftState = new GameState(grid, row - 1, col - 1);
                leftChildren.Add(newLeftState);
            }
            else if (row - 1 >= 0 && col + 1 < grid.GetLength(1) && !grid[row - 1, col + 1])
            {
                GameState newLeftState = new GameState(grid, row - 1, col + 1);
                leftChildren.Add(newLeftState);
            }
            else if (row + 1 < grid.GetLength(0) && col - 1 >= 0 && !grid[row + 1, col - 1])
            {
                GameState newLeftState = new GameState(grid, row + 1, col - 1);
                leftChildren.Add(newLeftState);
            }
            else if (row + 1 < grid.GetLength(0) && col + 1 < grid.GetLength(1) && !grid[row + 1, col + 1])
            {
                GameState newLeftState = new GameState(grid, row + 1, col + 1);
                leftChildren.Add(newLeftState);
            }
        }

        private void GenerateRightChildren()
        {
            // All valid positions to the left of the square.
            for (int j = col - 1; j >= 0 && !grid[row, j]; j--)
            {
                GameState newRightState = new GameState(grid, row, j);
                rightChildren.Add(newRightState);
            }
            // All valid positions to the right of the square.
            for (int j = col + 1; j < grid.GetLength(1) && !grid[row, j]; j++)
            {
                GameState newRightState = new GameState(grid, row, j);
                rightChildren.Add(newRightState);
            }
            // All valid positions above the square.
            for(int i = row - 1; i > 0 && !grid[i, col]; i--)
            {
                GameState newRightState = new GameState(grid, i, col);
                rightChildren.Add(newRightState);
            }
            // All valid positions below the square.
            for (int i = row + 1; i < grid.GetLength(0) && !grid[i, col]; i++)
            {
                GameState newRightState = new GameState(grid, i, col);
                rightChildren.Add(newRightState);
            }
        }

        public void GenerateRightChildrenSimple()
        {
            if(row - 1 >= 0 && !grid[row - 1, col])
            {
                GameState newRightState = new GameState(grid, row - 1, col);
                rightChildren.Add(newRightState);
            }
            else if (row + 1 < grid.GetLength(0) && !grid[row + 1, col])
            {
                GameState newRightState = new GameState(grid, row + 1, col);
                rightChildren.Add(newRightState);
            }
            if (col - 1 >= 0 && !grid[row, col - 1])
            {
                GameState newRightState = new GameState(grid, row, col - 1);
                rightChildren.Add(newRightState);
            }
            if (col + 1 < grid.GetLength(1) && !grid[row, col + 1])
            {
                GameState newRightState = new GameState(grid, row, col + 1);
                rightChildren.Add(newRightState);
            }
        }

        public bool[,] GetGrid()
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
        public int GetRow()
        {
            return row;
        }
        public int GetCol()
        {
            return col;
        }
    }
}
