using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Core_2048
{

    public class UpdateLoop
    {
        private readonly int _outerCount;
        private readonly int _innerCount;
        private readonly int _innerStart;
        private readonly int _innerEnd;

        private readonly int _baseValue;

        private readonly Drop _drop;
        private readonly Drop _reverseDrop;
        private readonly GetValue _getValue;
        private readonly SetValue _setValue;

        public UpdateLoop(bool isAlongRow, bool isIncreasing, int height, int width, int[,] elements,
            int baseValue)
        {
            _baseValue = baseValue;
            _outerCount = isAlongRow ? height : width;
            _innerCount = isAlongRow ? width : height;
            _innerStart = isIncreasing ? 0 : _innerCount - 1;
            _innerEnd = isIncreasing ? _innerCount - 1 : 0;

            _drop = Utils.DropFactory(isIncreasing);
            _reverseDrop = Utils.DropFactory(!isIncreasing);
            _getValue = Utils.GetValueFactory(isAlongRow, elements);
            _setValue = Utils.SetValueFactory(isAlongRow, elements);
        }

        public Dictionary<Element, Element> Loop(MergeElements newValue)
        {
            var updatingMap = new Dictionary<Element, Element>();
            for (var outerItem = 0; outerItem < _outerCount; outerItem++)
            {
                for (var innerItem = _innerStart;
                     Utils.IsInnerCondition(innerItem, _innerStart, _innerEnd);
                     innerItem = _reverseDrop(innerItem))
                {
                    if (_getValue(outerItem, innerItem).Value == _baseValue) continue;

                    var newItem = CalculateNewItem(innerItem, outerItem);
                    var update = UpdatingElement(newItem, innerItem, outerItem, newValue);
                    if (update != null)
                    {
                        updatingMap.Add(update.Item1, update.Item2);
                    }
                }
            }

            return updatingMap;
        }

        private int CalculateNewItem(int innerItem, int outerItem)
        {
            var newInnerItem = innerItem;
            do
            {
                newInnerItem = _drop(newInnerItem);
            } while (Utils.IsInnerCondition(newInnerItem, _innerStart, _innerEnd) &&
                     _getValue(outerItem, newInnerItem).Value == _baseValue);

            return newInnerItem;
        }

        [CanBeNull]
        private Tuple<Element, Element> UpdatingElement(int newInnerItem, int innerItem, int outerItem,
            MergeElements merge)
        {
            if (Utils.IsInnerCondition(newInnerItem, _innerStart, _innerEnd) &&
                _getValue(outerItem, newInnerItem).Value == _getValue(outerItem, innerItem).Value)
            {
                var newElement = merge(
                    _getValue(outerItem, newInnerItem).Value,
                    _getValue(outerItem, innerItem).Value
                );
                _setValue(outerItem, newInnerItem, newElement);
                _setValue(outerItem, innerItem, _baseValue);


                return new Tuple<Element, Element>(
                    _getValue(outerItem, innerItem),
                    _getValue(outerItem, newInnerItem)
                    );
            }

            newInnerItem = _reverseDrop(newInnerItem);
            var element = _getValue(outerItem, innerItem);
            _setValue(outerItem, innerItem, _baseValue);
            _setValue(outerItem, newInnerItem, element.Value);

            if (newInnerItem != innerItem)
            {
                return new Tuple<Element, Element>(
                    _getValue(outerItem, innerItem),
                    _getValue(outerItem, newInnerItem)
                    );
            }

            return null;
        }
    }

}
