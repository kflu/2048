using System;
using System.Collections;
using System.Collections.Generic;

namespace Core_2048
{

    public class UpdateLoop<T> : IEnumerable<UpdateLoop<T>.ChangeElementAction>
    {
        public delegate int Drop(int index);

        public delegate Cell<T> Get(int outerItem, int innerItem);

        public delegate bool IsInnerCondition(int index);

        private ICellBehavior<T> _cellBehavior;

        private Drop _drop;
        private Get _get;
        private int _innerEnd;
        private int _innerStart;
        private IsInnerCondition _isInnerCondition;

        private int _outerCount;

        private Drop _reverseDrop;

        public UpdateLoop(ICellBehavior<T> cellBehavior, Drop drop, Get get, int innerEnd, int innerStart,
            IsInnerCondition isInnerCondition,
            int outerCount, Drop reverseDrop)
        {
            _cellBehavior = cellBehavior ?? throw new ArgumentNullException(nameof(cellBehavior));
            _drop = drop ?? throw new ArgumentNullException(nameof(drop));
            _get = get ?? throw new ArgumentNullException(nameof(get));
            _innerEnd = innerEnd;
            _innerStart = innerStart;
            _isInnerCondition = isInnerCondition ?? throw new ArgumentNullException(nameof(isInnerCondition));
            _outerCount = outerCount;
            _reverseDrop = reverseDrop ?? throw new ArgumentNullException(nameof(reverseDrop));
        }

        public IEnumerator<ChangeElementAction> GetEnumerator()
        {
            for (var outerItem = 0; outerItem < _outerCount; outerItem++)
            {
                for (var innerItem = _innerStart; _isInnerCondition(innerItem); innerItem = _reverseDrop(innerItem))
                {
                    if (_cellBehavior.IsBaseCell(_get(outerItem, innerItem)))
                    {
                        continue;
                    }

                    var newInnerItem = CalculateNewItem(innerItem, outerItem);
                    var isMerge = _isInnerCondition(newInnerItem) && _cellBehavior.IsMergeCells(
                        _get(outerItem, newInnerItem),
                        _get(outerItem, innerItem)
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
            var previous = _get(outerItem, innerItem);
            var newElement = _get(outerItem, newInnerItem);

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
            newInnerItem = _reverseDrop(newInnerItem);
            var previous = _get(outerItem, innerItem);
            var newElement = _get(outerItem, newInnerItem);

            var next = new Cell<T>()
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
                newInnerItem = _drop(newInnerItem);
            } while (_isInnerCondition(newInnerItem) && _cellBehavior.IsBaseCell(_get(outerItem, newInnerItem)));

            return newInnerItem;
        }

        public class ChangeElementAction
        {
            public Cell<T> Next;
            public Cell<T> Previous;
        }
    }

}
