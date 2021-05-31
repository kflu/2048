using System;

using Core_2048;

namespace ConsoleOut
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var board = new Board(4, 4, 0);
            var elementGenerator = new RandomElementGenerator<ulong>();
            elementGenerator.EmptyChecker = element => element == 0;
            elementGenerator.AddToPool(2, 95);
            elementGenerator.AddToPool(4, 5);
            var app = Core.Builder()
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

                var isAlongRow = direction is Direction.Left or Direction.Right;
                var isIncreasing = direction is Direction.Left or Direction.Up;
                app.Update(isAlongRow, isIncreasing);
            }
        }

        private static void Render(Core<ulong> app)
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

            return key.Key switch
            {
                ConsoleKey.W => Direction.Up,
                ConsoleKey.S => Direction.Down,
                ConsoleKey.A => Direction.Left,
                ConsoleKey.D => Direction.Right,
                _            => null
            };
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
