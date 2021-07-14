using System;

using Core_2048;

namespace ConsoleOut
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var board = new Board<ulong>(4, 4, () => 0);
            var elementGenerator = new RandomCellGenerator<ulong>(element => element == 0);
            elementGenerator.AddToPool(2, 95);
            elementGenerator.AddToPool(4, 5);
            var app = new BoardBehavior<ulong>(board, elementGenerator, new BaseCellBehavior());
            app.AddNew();
            app.AddUpdatedListener(elements =>
            {
                app.AddNew();
                Render(board);
            });
            Render(board);
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

        private static void Render(Board<ulong> board)
        {
            Console.Clear();
            var prevRow = -1;
            var pattern = new Func<ulong, string>(value => $"  {value}  |");
            board.ForEach((value, row, column) =>
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

    internal class BaseCellBehavior : ICellBehavior<ulong>
    {
        private readonly ulong _baseValue;

        public BaseCellBehavior(ulong baseValue = 0)
        {
            _baseValue = baseValue;
        }

        public bool IsBaseCell(Cell<ulong> cell)
        {
            return cell.Value == _baseValue;
        }

        public bool IsMergeCells(Cell<ulong> previous, Cell<ulong> next)
        {
            return previous.Value == next.Value;
        }

        public ulong MergeCells(Cell<ulong> previous, Cell<ulong> next)
        {
            return previous.Value + next.Value;
        }

        public ulong GetCellBaseValue()
        {
            return _baseValue;
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
