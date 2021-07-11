using System;
using System.Collections;
using System.Collections.Generic;

namespace Core_2048
{

    public partial class BoardBehavior<T>
    {
        private class UpdateLoop : IEnumerable<ChangeElementAction>
        {
            private readonly ICellBehavior<T> _cellBehavior;
            private readonly Board<T> _board;
            private readonly bool _isIncreasing;
            private readonly bool _isAlongRow;

            private int OuterCount => _isAlongRow ? _board.Height : _board.Width;
            private int InnerCount => _isIncreasing ? _board.Width : _board.Height;

            private int InnerStart => _isIncreasing ? 0 : InnerCount - 1;
            private int InnerEnd => _isIncreasing ? InnerCount - 1 : 0;

            public UpdateLoop(ICellBehavior<T> cellBehavior, Board<T> board, bool isAlongRow, bool isIncreasing)
            {
                _cellBehavior = cellBehavior ?? throw new ArgumentNullException(nameof(cellBehavior));
                _board = board ?? throw new ArgumentNullException(nameof(board));
                _isAlongRow = isAlongRow;
                _isIncreasing = isIncreasing;
            }

            public IEnumerator<ChangeElementAction> GetEnumerator()
            {
                for (var outerItem = 0; outerItem < OuterCount; outerItem++)
                {
                    for (var innerItem = InnerStart; IsInnerCondition(innerItem); innerItem = ReverseDrop(innerItem))
                    {
                        if (_cellBehavior.IsBaseCell(Get(outerItem, innerItem)))
                        {
                            continue;
                        }

                        var newInnerItem = CalculateNewItem(innerItem, outerItem);
                        var isMerge = IsInnerCondition(newInnerItem) && _cellBehavior.IsMergeCells(
                            Get(outerItem, newInnerItem),
                            Get(outerItem, innerItem)
                        );

                        yield return isMerge
                            ? ExecuteWithMerge(outerItem, innerItem, newInnerItem)
                            : ExecuteWithoutMerge(outerItem, innerItem, newInnerItem);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private ChangeElementAction ExecuteWithMerge(int outerItem, int innerItem, int newInnerItem)
            {
                var previous = Get(outerItem, innerItem);
                var newElement = Get(outerItem, newInnerItem);

                var next = new Cell<T>
                {
                    Row = newElement.Row,
                    Column = newElement.Column,
                    Value = _cellBehavior.MergeCells(previous, newElement)
                };

                return new ChangeElementAction
                {
                    Previous = previous,
                    Next = next
                };
            }

            private ChangeElementAction ExecuteWithoutMerge(int outerItem, int innerItem, int newInnerItem)
            {
                newInnerItem = ReverseDrop(newInnerItem);
                var previous = Get(outerItem, innerItem);
                var newElement = Get(outerItem, newInnerItem);

                var next = new Cell<T>
                {
                    Row = newElement.Row,
                    Column = newElement.Column,
                    Value = previous.Value
                };

                return new ChangeElementAction
                {
                    Previous = previous,
                    Next = next
                };
            }

            private int CalculateNewItem(int innerItem, int outerItem)
            {
                var newInnerItem = innerItem;
                do
                {
                    newInnerItem = Drop(newInnerItem);
                } while (IsInnerCondition(newInnerItem) && _cellBehavior.IsBaseCell(Get(outerItem, newInnerItem)));

                return newInnerItem;
            }

            private Cell<T> Get(int outerItem, int innerItem)
            {
                return _isAlongRow
                    ? new Cell<T> { Row = outerItem, Column = innerItem, Value = _board.Get(outerItem, innerItem) }
                    : new Cell<T> { Row = innerItem, Column = outerItem, Value = _board.Get(innerItem, outerItem) };
            }

            private int Drop(int innerIndex)
            {
                return _isIncreasing ? innerIndex - 1 : innerIndex + 1;
            }

            private int ReverseDrop(int innerIndex)
            {
                return !_isIncreasing ? innerIndex - 1 : innerIndex + 1;
            }

            private bool IsInnerCondition(int index)
            {
                var minIndex = Math.Min(InnerStart, InnerEnd);
                var maxIndex = Math.Max(InnerStart, InnerEnd);

                return minIndex <= index && index <= maxIndex;
            }
        }
    }

}
