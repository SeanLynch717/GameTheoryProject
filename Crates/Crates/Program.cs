 using System;
using System.Collections.Generic;

namespace Crates
{
    class Program
    {
        static Cell[,] grid;
        static Cell p1Start = new Cell(new List<ItemType> { ItemType.Crate });
        static Cell p2Start = new Cell(new List<ItemType> { ItemType.Guard, ItemType.End });
        static Cell leftConveyer = new Cell(new List<ItemType> { ItemType.Conveyer }, Direction.Left);
        static Cell rightConveyer = new Cell(new List<ItemType> { ItemType.Conveyer }, Direction.Right);
        static Cell upConveyer = new Cell(new List<ItemType> { ItemType.Conveyer }, Direction.Up);
        static Cell downConveyer = new Cell(new List<ItemType> { ItemType.Conveyer }, Direction.Down);
        static Cell emptyCell = new Cell(new List<ItemType> { ItemType.None });
        static Cell endZone = new Cell(new List<ItemType> { ItemType.End });

        public static Dictionary<int, string> outcomeLookUp = new Dictionary<int, string>();


        static void Main(string[] args)
        {
            List<Cell[,]> grids = new List<Cell[,]>();

            // Impartial Grids.
            //grid = new Cell[,]{
            //    { p1Start, emptyCell, emptyCell},
            //    { emptyCell, emptyCell, emptyCell},
            //    { emptyCell, emptyCell, endZone}
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, endZone }
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell, endZone }
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell, emptyCell, endZone }
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell, emptyCell, emptyCell, endZone }
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell, emptyCell, emptyCell, emptyCell, endZone }
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell, emptyCell, emptyCell, emptyCell, emptyCell, endZone }
            //};
            //grids.Add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell, emptyCell, emptyCell, emptyCell, emptyCell, emptyCell, endZone }
            //};
            //grids.Add(grid);

            /*
            grid = new Cell[,]{
                {p1Start, emptyCell },
                {emptyCell, endZone }
            };
            grids.Add(grid);

            grid = new Cell[,]{
                {p1Start, emptyCell, null },
                {emptyCell, emptyCell, endZone }
            };
            grids.Add(grid);

            grid = new Cell[,]{
                {p1Start, emptyCell, emptyCell },
                {emptyCell, emptyCell, endZone }
            };
            grids.Add(grid);

            grid = new Cell[,]{
                {p1Start, emptyCell, emptyCell, emptyCell },
                {emptyCell, emptyCell, emptyCell, endZone }
            };
            grids.Add(grid);

            */
            //grid = new Cell[,]{
            //    {p1Start, emptyCell, emptyCell },
            //    {emptyCell, emptyCell, emptyCell },
            //    {emptyCell, emptyCell, endZone }
            //};
            //grids.Add(grid);

            //grid = new cell[,]{
            //    {p1start, emptycell },
            //    {emptycell, emptycell },
            //    {null, endzone }
            //};
            //grids.add(grid);

            //grid = new cell[,]{
            //    {p1start, emptycell },
            //    {emptycell, emptycell },
            //    {emptycell, endzone }
            //};
            //grids.add(grid);

            //grid = new Cell[,]{
            //    {p1Start, emptyCell },
            //    {emptyCell, emptyCell },
            //    {emptyCell, emptyCell },
            //    {emptyCell, endZone },
            //};
            //grids.Add(grid);


            // Partizan Grids
            grid = new Cell[,]{
                { p1Start, emptyCell },
                { emptyCell, p2Start }
                };
            grids.Add(grid);
            //grid = new Cell[,]{
            //   { p1Start, emptyCell, emptyCell },
            //   { emptyCell, emptyCell, p2Start },
            //};
            //grid = new Cell[,]{
            //    { p1Start, emptyCell, p2Start }
            // };

            // Loop through each game and solve it.
            foreach (Cell[,] currentGrid in grids)
            {
                GameState root = new GameState(currentGrid, 0, 0, 2, 2);
                Console.WriteLine("Starting configuration:\n");
                root.Print();
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

                    // Generate all children of current.
                    current.GenerateChildren();

                    // Enqueue each child of current
                    foreach (GameState g in current.GetLeftChildren())
                    {
                        queue.Enqueue(g);
                    }
                    foreach (GameState g in current.GetRightChildren())
                    {
                        queue.Enqueue(g);
                    }

                    if (count % 1000 == 0)
                    {
                        //Console.WriteLine((double)stached / count);
                    }
                }

                //PrintPathsRecur(root, new List<GameState>());

                if (root.impartial)
                {
                    root.FindOutComeClassImpartial();
                    Console.WriteLine("First Players Options");
                    List<string> options = new List<string>();
                    foreach(GameState g in root.GetLeftChildren())
                    {
                        options.Add(g.GetOutcomeClass());
                        Console.Write("| ");
                    }
                    Console.WriteLine();
                    foreach(string s in options)
                    {
                        Console.Write(s + " ");
                    }

                    Console.WriteLine("\n\nOutcome class: " + root.GetOutcomeClass() + "\n\n\n");
                }
                else
                {
                    root.FindOutComeClass();
                    Console.WriteLine("Left:");
                    foreach (GameState g in root.GetLeftChildren())
                    {
                        Console.Write(g.GetOutcomeClass() + " ");
                    }
                    Console.WriteLine("\n\nRight:");
                    foreach (GameState g in root.GetRightChildren())
                    {
                        Console.Write(g.GetOutcomeClass() + " ");
                    }

                    Console.WriteLine("\n\nOutcome class: " + root.GetOutcomeClass() + "\n\n\n");
                }

                //outcomeLookUp = new Dictionary<int, string>();
            }
        }
        public static void PrintPathsRecur(GameState node, List<GameState> copypath)
        {
            List<GameState> path = new List<GameState>();
            foreach(GameState g in copypath)
            {
                path.Add(g);
            }
            /* append this node to the path array */
            path.Add(node);

            /* it's a leaf, so print the path that led to here  */
            if (node.GetLeftChildren().Count == 0 && node.GetRightChildren().Count == 0)
            {
                PrintArray(path);
            }
            else
            {
                /* otherwise try both subtrees */
                foreach (GameState g in node.GetLeftChildren())
                {
                    PrintPathsRecur(g, path);
                }
                foreach (GameState g in node.GetRightChildren())
                {
                    PrintPathsRecur(g, path);
                }
            }
        }

        public static void PrintArray(List<GameState> path)
        {
            Console.WriteLine("New Path: ");
            foreach(GameState g in path)
            {
                Console.WriteLine(g.LastToMove);
                g.Print();
            }
            Console.WriteLine();
        }
    }
}
