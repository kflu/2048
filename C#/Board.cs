using System;
using System.Collections;
using System.Collections.Generic;

namespace Core_2048
{

    public partial class Core<T>
    {
        /// <inheritdoc />
        public class Board : IEnumerable<Element>
        {
            /// <summary>
            ///     Delegate to interact with value of a board element without changing its position on board
            /// </summary>
            public delegate void Mapper(T value, int row, int column);

            private readonly T[,] _values;

            /// <summary>
            ///     Constructor create array elements
            /// </summary>
            /// <param name="height">outer array dimensions</param>
            /// <param name="width">inner array dimensions</param>
            /// <param name="valueInitiator">value for initial array elements</param>
            public Board(int height, int width, Func<T> valueInitiator)
            {
                Height = height;
                Width = width;

                _values = new T[Height, Width];
                ForEach((value, row, column) => Set(row, column, valueInitiator()));
            }

            /// <summary>
            ///     Amount elements for outer array dimensions: T[Height, Width]
            /// </summary>
            public int Height { get; }

            /// <summary>
            ///     Amount elements for inner array dimensions: T[Height, Width]
            /// </summary>
            public int Width { get; }

            /// <inheritdoc />
            public IEnumerator<Element> GetEnumerator()
            {
                for (var row = 0; row < _values.GetLength(0); row++)
                {
                    for (var column = 0; column < _values.GetLength(1); column++)
                    {
                        yield return new Element
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

            /// <summary>
            ///     Get origin value of element
            /// </summary>
            /// <param name="row">Outer array dimension</param>
            /// <param name="column">Inner array dimension</param>
            /// <returns>Value of element</returns>
            public T Get(int row, int column)
            {
                return _values[row, column];
            }

            /// <summary>
            ///     Set value of element
            /// </summary>
            /// <param name="row">Outer array dimension</param>
            /// <param name="column">Inner array dimension</param>
            /// <param name="value">Value of element</param>
            /// <returns>This board</returns>
            public Board Set(int row, int column, T value)
            {
                _values[row, column] = value;

                return this;
            }

            /// <summary>
            ///     Set element instead of another element with the same indexes
            /// </summary>
            /// <param name="element">Element with value</param>
            /// <returns>This board</returns>
            public Board Set(Element element)
            {
                Set(element.Row, element.Column, element.Value);

                return this;
            }

            /// <summary>
            ///     Invoke mapper for each elements
            /// </summary>
            /// <seealso cref="List{T}.ForEach" />
            /// <param name="mapper">Delegate to invoke</param>
            public void ForEach(Mapper mapper)
            {
                foreach (var element in this)
                {
                    mapper.Invoke(element.Value, element.Row, element.Column);
                }
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (!(obj is Board anotherBoard))
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

            /// <summary>
            ///     Determines whether the specified board is equal to the current object.
            /// </summary>
            /// <param name="other">another board</param>
            /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
            protected bool Equals(Board other)
            {
                return Equals(_values, other._values) && Height == other.Height && Width == other.Width;
            }

            /// <inheritdoc />
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

            /// <summary>
            ///     Determines if the boards are equal to each other.
            /// </summary>
            /// <param name="a">first board</param>
            /// <param name="b">second board</param>
            /// <returns>true if is equals; otherwise, false</returns>
            public static bool operator ==(Board a, Board b)
            {
                return Equals(a, b);
            }

            /// <summary>
            ///     Determines if the boards are not equal to each other.
            /// </summary>
            /// <param name="a">first board</param>
            /// <param name="b">second board</param>
            /// <returns>true if is not equals; otherwise, true</returns>
            public static bool operator !=(Board a, Board b)
            {
                return !Equals(a, b);
            }
        }
    }

}
