using System;

using Core_2048;

namespace ConsoleOut
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var board = new Board<ulong>(4, 4, () => 0);
            var elementGenerator = RandomCellGenerator<ulong>.Builder()
                .SetEmptyChecker(element => element == 0)
                .Build();
            elementGenerator.AddToPool(2, 95);
            elementGenerator.AddToPool(4, 5);
            var app = BoardBehavior<ulong>.Builder()
                .SetBoard(board)
                .SetBaseValue(0)
                .SetMerge((value, oldValue) => value + oldValue)
                .SetPredictor((current, target) => current == target)
                .SetElementGenerator(elementGenerator)
                .Build();
            app.AddNew();
            app.Updated += elements =>
            {
                app.AddNew();
                Render(app);
            };
            Render(app);
            while (true)
            {
                var direction = Input();
                if (direction == null)
                {
                    continue;
                }

                var isAlongRow = direction == Direction.Left || direction == Direction.Right;
                var isIncreasing = direction == Direction.Left || direction == Direction.Up;
                app.Update(isAlongRow, isIncreasing);
            }
        }

        private static void Render(BoardBehavior<ulong> app)
        {
            Console.Clear();
            var prevRow = -1;
            var pattern = new Func<ulong, string>(value => $"  {value}  |");
            app.Board.ForEach((value, row, column) =>
            {
                var cell = pattern(value);
                if (prevRow == row)
                {
                    Console.Write(cell);
                }
                else
                {
                    Console.WriteLine("");
                    Console.Write($"|{cell}");
                    prevRow = row;
                }
            });
        }

        private static Direction? Input()
        {
            var key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.W:
                    return Direction.Up;
                case ConsoleKey.S:
                    return Direction.Down;
                case ConsoleKey.A:
                    return Direction.Left;
                case ConsoleKey.D:
                    return Direction.Right;
                default:
                    return null;
            }
        }
    }

    internal enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

}
