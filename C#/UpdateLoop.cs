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

        #region Builder

        public static UpdateLoopBuilder Builder()
        {
            return new UpdateLoopBuilder();
        }

        public class UpdateLoopBuilder
        {
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

            public UpdateLoopBuilder SetDrop(Drop drop)
            {
                _drop = drop;

                return this;
            }

            public UpdateLoopBuilder SetReverseDrop(Drop reverseDrop)
            {
                _reverseDrop = reverseDrop;

                return this;
            }

            public UpdateLoopBuilder SetMerge(Merge merge)
            {
                _merge = merge;

                return this;
            }

            public UpdateLoopBuilder SetGetter(Get get)
            {
                _get = get;

                return this;
            }

            public UpdateLoopBuilder SetPredictor(Predictor predictor)
            {
                _predictor = predictor;

                return this;
            }

            public UpdateLoopBuilder SetOuterCount(int outerCount)
            {
                _outerCount = outerCount;

                return this;
            }

            public UpdateLoopBuilder SetInnerCondition(int innerStart, int innerEnd)
            {
                _isInnerCondition = index =>
                {
                    var minIndex = Math.Min(innerStart, innerEnd);
                    var maxIndex = Math.Max(innerStart, innerEnd);

                    return minIndex <= index && index <= maxIndex;
                };
                _innerStart = innerStart;
                _innerEnd = innerEnd;

                return this;
            }

            public UpdateLoopBuilder SetBaseValue(T baseValue)
            {
                _baseValue = baseValue;

                return this;
            }

            public UpdateLoop<T> Build()
            {
                return new UpdateLoop<T>
                {
                    _drop = _drop,
                    _reverseDrop = _reverseDrop,
                    _merge = _merge,
                    _get = _get,
                    _outerCount = _outerCount,
                    _isInnerCondition = _isInnerCondition,
                    _innerStart = _innerStart,
                    _innerEnd = _innerEnd,
                    _predictor = _predictor,
                    _baseValue = _baseValue
                };
            }
        }

        #endregion
    }

}
