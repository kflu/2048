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

        public delegate T Merge(T newValue, T oldValue);

        public delegate bool Predictor(T current, T target);

        private T _baseValue;

        private Drop _drop;
        private Get _get;
        private int _innerEnd;
        private int _innerStart;
        private IsInnerCondition _isInnerCondition;
        private Merge _merge;

        private int _outerCount;

        private Predictor _predictor;
        private Drop _reverseDrop;

        public UpdateLoop(T baseValue, Drop drop, Get get, int innerEnd, int innerStart,
            IsInnerCondition isInnerCondition, Merge merge, int outerCount, Predictor predictor, Drop reverseDrop)
        {
            if (baseValue == null)
            {
                throw new ArgumentNullException(nameof(baseValue));
            }

            _baseValue = baseValue;
            _drop = drop ?? throw new ArgumentNullException(nameof(drop));
            _get = get ?? throw new ArgumentNullException(nameof(get));
            _innerEnd = innerEnd;
            _innerStart = innerStart;
            _isInnerCondition = isInnerCondition ?? throw new ArgumentNullException(nameof(isInnerCondition));
            _merge = merge ?? throw new ArgumentNullException(nameof(merge));
            _outerCount = outerCount;
            _predictor = predictor ?? throw new ArgumentNullException(nameof(predictor));
            _reverseDrop = reverseDrop ?? throw new ArgumentNullException(nameof(reverseDrop));
        }

        public IEnumerator<ChangeElementAction> GetEnumerator()
        {
            for (var outerItem = 0; outerItem < _outerCount; outerItem++)
            {
                for (var innerItem = _innerStart; _isInnerCondition(innerItem); innerItem = _reverseDrop(innerItem))
                {
                    if (Equals(_get(outerItem, innerItem).Value, _baseValue))
                    {
                        continue;
                    }

                    var newInnerItem = CalculateNewItem(innerItem, outerItem);
                    var isMerge = _isInnerCondition(newInnerItem) && _predictor(
                        _get(outerItem, newInnerItem).Value,
                        _get(outerItem, innerItem).Value
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
                Value = _merge(newElement.Value, previous.Value)
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
            } while (_isInnerCondition(newInnerItem) && Equals(_get(outerItem, newInnerItem).Value, _baseValue));

            return newInnerItem;
        }

        public class ChangeElementAction
        {
            public Cell<T> Next;
            public Cell<T> Previous;
        }
    }

}
