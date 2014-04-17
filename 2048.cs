using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kfl.Game2048
{
    class Program
    {
        static private readonly Random random = new Random();

        static void Main(string[] args)
        {
            bool hasUpdated = true;
            int[,] canvas = new int[4, 4];
            do
            {
                if (hasUpdated)
                {
                    PutNewValue(canvas);
                }

                Display(canvas);

                if (IsDead(canvas))
                {
                    using (new ColorOutput(ConsoleColor.Red))
                    {
                        Console.WriteLine("YOU ARE DEAD!!!");
                        break;
                    }
                }

                Console.WriteLine("Use arrow keys to move the tiles. Press Ctrl-C to exit.");
                ConsoleKeyInfo input = Console.ReadKey(true); // BLOCKING TO WAIT FOR INPUT
                Console.WriteLine(input.Key.ToString());

                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        hasUpdated = Update(canvas, Direction.Up);
                        break;

                    case ConsoleKey.DownArrow:
                        hasUpdated = Update(canvas, Direction.Down);
                        break;

                    case ConsoleKey.LeftArrow:
                        hasUpdated = Update(canvas, Direction.Left);
                        break;

                    case ConsoleKey.RightArrow:
                        hasUpdated = Update(canvas, Direction.Right);
                        break;

                    default:
                        hasUpdated = false;
                        break;
                }
            }
            while (true); // use CTRL-C to break out of loop

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }

        #region Utility methods

        private static bool IsDead(int[,] canvas)
        {
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                int[,] clone = (int[,])canvas.Clone();
                if (Update(clone, dir))
                {
                    return false;
                }
            }

            // tried all directions. none worked.
            return true;
        }

        private static ConsoleColor GetNumberColor(int num)
        {
            switch (num)
            {
                case 0:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.Cyan;
                case 4:
                    return ConsoleColor.Magenta;
                case 8:
                    return ConsoleColor.Red;
                case 16:
                    return ConsoleColor.Green;
                case 32:
                    return ConsoleColor.Yellow;
                case 64:
                    return ConsoleColor.Yellow;
                case 128:
                    return ConsoleColor.DarkCyan;
                case 256:
                    return ConsoleColor.Cyan;
                case 512:
                    return ConsoleColor.DarkMagenta;
                case 1024:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.Red;
            }
        }

        private static void Display(int[,] canvas)
        {
            int nRows = canvas.GetLength(0);
            int nCols = canvas.GetLength(1);

            Console.Clear();
            Console.WriteLine();
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    using (new ColorOutput(GetNumberColor(canvas[i, j])))
                    {
                        Console.Write(string.Format("{0,6}", canvas[i, j]));
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private static bool Update(int[,] canvas, Direction direction)
        {
            int nRows = canvas.GetLength(0);
            int nCols = canvas.GetLength(1);
            bool hasUpdated = false;

            // You shouldn't be dead at this point. We always check if you're dead at the end of the Update()

            // Drop along row or column? true: process inner along row; false: process inner along column
            bool isAlongRow = direction == Direction.Left || direction == Direction.Right;

            // Should we process inner dimension in increasing index order?
            bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

            int outterCount = isAlongRow ? nRows : nCols;
            int innerCount = isAlongRow ? nCols : nRows;

            int innerStart = isIncreasing ? 0 : innerCount - 1;
            int innerEnd = isIncreasing ? innerCount - 1 : 0;
            Func<int, int> drop = isIncreasing ? new Func<int, int>(innerIndex => innerIndex - 1) : new Func<int, int>(innerIndex => innerIndex + 1);
            Func<int, int> reverseDrop = isIncreasing ? new Func<int, int>(innerIndex => innerIndex + 1) : new Func<int, int>(innerIndex => innerIndex - 1);

            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

            Func<int[,], int, int, int> getValue = isAlongRow
                ? new Func<int[,], int, int, int>((x, i, j) => x[i, j])
                : new Func<int[,], int, int, int>((x, i, j) => x[j, i]);

            Action<int[,], int, int, int> setValue = isAlongRow
                ? new Action<int[,], int, int, int>((x, i, j, v) => x[i, j] = v)
                : new Action<int[,], int, int, int>((x, j, i, v) => x[i, j] = v);

            for (int i = 0; i < outterCount; i++)
            {
                bool mergeOccurred = false;
                for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                {
                    if (getValue(canvas, i, j) == 0)
                    {
                        continue;
                    }

                    int newJ = j;
                    do
                    {
                        newJ = drop(newJ);
                    }
                    // Continue probing along as long as we haven't hit the boundary and the new position isn't occupied
                    while (innerCondition(newJ) && getValue(canvas, i, newJ) == 0);

                    if (innerCondition(newJ) && !mergeOccurred && getValue(canvas, i, newJ) == getValue(canvas, i, j))
                    {
                        // We did not hit the canvas boundary (we hit a node) AND no previous merge occurred AND the nodes' values are the same
                        // Let's merge
                        mergeOccurred = true;
                        setValue(
                            canvas,
                            i,
                            newJ,
                            2 * getValue(canvas, i, newJ));
                        setValue(canvas, i, j, 0);
                        hasUpdated = true;
                    }
                    else
                    {
                        // Reached the boundary OR...
                        // we hit a node with different value OR...
                        // we hit a node with same value BUT a prevous merge had occurred
                        // 
                        // Simply stack along
                        newJ = reverseDrop(newJ); // reverse back to its valid position
                        if (newJ != j)
                        {
                            // there's an update
                            hasUpdated = true;
                        }

                        int value = getValue(canvas, i, j);
                        setValue(canvas, i, j, 0);
                        setValue(canvas, i, newJ, value);
                    }
                }
            }

            return hasUpdated;
        }

        private static void PutNewValue(int[,] canvas)
        {
            int nRows = canvas.GetLength(0);
            int nCols = canvas.GetLength(1);

            // Find all empty slots
            List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();
            for (int iRow = 0; iRow < nRows; iRow++)
            {
                for (int iCol = 0; iCol < nCols; iCol++)
                {
                    if (canvas[iRow, iCol] == 0)
                    {
                        emptySlots.Add(new Tuple<int, int>(iRow, iCol));
                    }
                }
            }

            // We should have at least 1 empty slot. Since we know the user is not dead
            int iSlot = random.Next(0, emptySlots.Count); // randomly pick an empty slot
            int value = random.Next(0, 100) < 95 ? 2 : 4; // randomly pick 2 (with 95% chance) or 4 (rest of the chance)
            canvas[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2] = value;
        }

        #endregion Utility methods

        #region Utility classes

        enum Direction
        {
            Up,
            Down,
            Right,
            Left,
        }

        class ColorOutput : IDisposable
        {
            public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
            }

            public void Dispose()
            {
                Console.ResetColor();
            }
        }

        #endregion Utility classes
    }
}