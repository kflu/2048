using System;

using Core_2048;

namespace ConsoleOut
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var app = new Core(4, 4, 0, 0,
                (value, oldValue) => value * 2
                );
            app.SetValue(new Element(2, 2, app.StandardNewValue));
            Render(app);
            while (true)
            {
                var direction = Input();
                if (direction != null)
                {
                    var isAlongRow = direction is Direction.Left or Direction.Right;
                    var isIncreasing = direction is Direction.Left or Direction.Up;
                    app.UpdateElements(isAlongRow, isIncreasing);
                }

                if (app.HasUpdated)
                {
                    var newRandom = app.NewElement();
                    if (newRandom != null)
                    {
                        app.SetValue(newRandom);
                    }
                }
                Render(app);
            }
        }

        private static void Render(Core app)
        {
            Console.Clear();
            var prevRow = -1;
            var pattern = new Func<int, string>(value => $"  {value}  |");
            app.ForEach(element =>
            {
                var cell = pattern(element.Value);
                if (prevRow == element.Row)
                {
                    Console.Write(cell);
                }
                else
                {
                    Console.WriteLine("");
                    Console.Write($"|{cell}");
                    prevRow = element.Row;
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
