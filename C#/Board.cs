using System;
using System.Collections;
using System.Collections.Generic;

namespace Core_2048
{

    public class Board<T> : IEnumerable<Cell<T>>
    {
        public delegate void Mapper(T value, int row, int column);

        private readonly T[,] _values;

        public Board(int height, int width, Func<T> valueInitiator)
        {
            Height = height;
            Width = width;

            _values = new T[Height, Width];
            ForEach((value, row, column) => Set(row, column, valueInitiator()));
        }

        public int Height { get; }
        public int Width { get; }

        public IEnumerator<Cell<T>> GetEnumerator()
        {
            for (var row = 0; row < _values.GetLength(0); row++)
            {
                for (var column = 0; column < _values.GetLength(1); column++)
                {
                    yield return new Cell<T>()
                    {
                        Row = row,
                        Column = column,
                        Value = _values[row, column]
                    };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Get(int row, int column)
        {
            return _values[row, column];
        }

        public Board<T> Set(int row, int column, T value)
        {
            _values[row, column] = value;

            return this;
        }

        public Board<T> Set(Cell<T> cell)
        {
            Set(cell.Row, cell.Column, cell.Value);

            return this;
        }

        public void ForEach(Mapper mapper)
        {
            foreach (var element in this)
            {
                mapper.Invoke(element.Value, element.Row, element.Column);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Board<T> anotherBoard))
            {
                return false;
            }

            if (Height != anotherBoard.Height)
            {
                return false;
            }

            if (Width != anotherBoard.Width)
            {
                return false;
            }

            var result = true;

            ForEach((value, row, column) =>
            {
                var anotherValue = anotherBoard.Get(row, column);
                result = result
                         && value.GetType() == anotherValue.GetType()
                         && Equals(value, anotherValue)
                         && result;
            });

            return result;
        }

        protected bool Equals(Board<T> other)
        {
            return Equals(_values, other._values) && Height == other.Height && Width == other.Width;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _values != null ? _values.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Width;

                return hashCode;
            }
        }
    }

}
