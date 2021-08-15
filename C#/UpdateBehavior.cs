using System;

namespace Core_2048
{

    public partial class Core<T>
    {
        internal class UpdateBehavior
        {
            private readonly Board _board;
            private readonly bool _isAlongRow;
            private readonly bool _isIncreasing;

            public UpdateBehavior(Board board, bool isIncreasing, bool isAlongRow)
            {
                _board = board ?? throw new ArgumentNullException(nameof(board));
                _isIncreasing = isIncreasing;
                _isAlongRow = isAlongRow;
            }

            public int OuterCount => _isAlongRow ? _board.Height : _board.Width;
            public int InnerCount => _isIncreasing ? _board.Width : _board.Height;

            public int InnerStart => _isIncreasing ? 0 : InnerCount - 1;
            public int InnerEnd => _isIncreasing ? InnerCount - 1 : 0;

            public int Drop(int index)
            {
                return _isIncreasing ? index - 1 : index + 1;
            }

            public int ReverseDrop(int index)
            {
                return !_isIncreasing ? index - 1 : index + 1;
            }

            public Element Get(int outerItem, int innerItem)
            {
                return _isAlongRow
                    ? new Element { Row = outerItem, Column = innerItem, Value = _board.Get(outerItem, innerItem) }
                    : new Element { Row = innerItem, Column = outerItem, Value = _board.Get(innerItem, outerItem) };
            }

            public bool IsInnerCondition(int index)
            {
                var minIndex = Math.Min(InnerStart, InnerEnd);
                var maxIndex = Math.Max(InnerStart, InnerEnd);

                return minIndex <= index && index <= maxIndex;
            }
        }
    }

}
