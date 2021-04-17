using System;
using System.Collections.Generic;


namespace Core_2048
{

    public delegate int ChangeElement(Element element);
    public delegate void ActionByElement(Element element);
    public delegate int MergeElements(int newValue, int oldValue);
    internal delegate int Drop(int index);
    internal delegate Element GetValue(int outerItem, int innerItem);
    internal delegate void SetValue(int outerItem, int innerItem, int value);

    public class Core
    {
        public int Width { get; }
        public int Height { get; }
        public bool HasUpdated { get; private set; }
        public int Score { get; private set; }

        public Dictionary<Element, Element> UpdatingMap { get; private set; }

        public int ChanceBetterValue = 95;
        public int StandardNewValue = 2;
        public int BetterNewValue = 4;

        private readonly int[,] _elements;
        private readonly int _baseValue = 0;
        private readonly Random _random = new Random();
        private readonly MergeElements _mergeElements;

        public Core(int width, int height, int? startScore, int? baseValue, MergeElements mergeElements)
        {
            Width = width;
            Height = height;
            Score = startScore ?? 0;
            _elements = new int[Height,Width];
            _baseValue = baseValue ?? _baseValue;
            _mergeElements = mergeElements;
            MapperElements(element => _baseValue);
        }

        public int GetValue(int row, int column)
        {
            return _elements[row, column];
        }

        public Element SetValue(Element element)
        {
            _elements[element.Row, element.Column] = element.Value;
            return element;
        }

        public void UpdateElements(bool isAlongRow, bool isIncreasing)
        {
            var outerCount = isAlongRow ? Height : Width;
            var innerCount = isAlongRow ? Width : Height;
            var innerStart = isIncreasing ? 0 : innerCount - 1;
            var innerEnd = isIncreasing ? innerCount - 1 : 0;

            HasUpdated = false;
            var updatingMap = new Dictionary<Element, Element>();

            var drop = DropFactory(isIncreasing);
            var reverseDrop = DropFactory(!isIncreasing);
            var getValue = GetValueFactory(isAlongRow);
            var setValue = SetValueFactory(isAlongRow);

            for (var outerItem = 0; outerItem < outerCount; outerItem++)
            {
                for (var innerItem = innerStart; IsInnerCondition(innerItem, innerStart, innerEnd); innerItem = reverseDrop(innerItem))
                {
                    if (getValue(outerItem, innerItem).Value == _baseValue) continue;

                    var newInnerItem = CalculateNewItem(innerItem, drop, innerStart, innerEnd, getValue, outerItem);
                    var isInnerCondition = IsInnerCondition(newInnerItem, innerStart, innerEnd);

                    if (isInnerCondition
                        && getValue(outerItem, newInnerItem).Value == getValue(outerItem, innerItem).Value)
                    {
                        var newElement = _mergeElements(
                            getValue(outerItem, newInnerItem).Value,
                            getValue(outerItem, innerItem).Value
                            );
                        setValue(outerItem, newInnerItem, newElement);
                        setValue(outerItem, innerItem, _baseValue);
                        HasUpdated = true;
                        updatingMap.Add(getValue(outerItem, innerItem), getValue(outerItem, newInnerItem));
                        Score += newElement;
                    }
                    else
                    {
                        newInnerItem = reverseDrop(newInnerItem);

                        var element = getValue(outerItem, innerItem);
                        setValue(outerItem, innerItem, _baseValue);
                        setValue(outerItem, newInnerItem, element.Value);

                        if (newInnerItem == innerItem) continue;

                        HasUpdated = true;
                        updatingMap.Add(getValue(outerItem, innerItem), getValue(outerItem, newInnerItem));
                    }
                }
            }

            UpdatingMap = updatingMap;
        }

        public bool CheckIfEmpty(int row, int column)
        {
            return GetValue(row, column) == _baseValue;
        }

        public bool IsGameOver()
        {
            var result = true;
            ForEach(element =>
            {
                if (element.Row < Height - 1
                    && GetValue(element.Row, element.Column) == GetValue(element.Row, element.Column + 1))
                {
                    result = false;
                }

                if (element.Column < Width - 1
                    && GetValue(element.Row, element.Column) == GetValue(element.Row + 1, element.Column))
                {
                    result = false;
                }
            });
            return result;
        }

        public Element NewElement()
        {
            var empties = new List<Tuple<int, int>>();
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    if (CheckIfEmpty(row, column))
                    {
                        empties.Add(new Tuple<int, int>(row, column));
                    }
                }
            }

            var index = _random.Next(0, empties.Count);
            var value = _random.Next(0, 100) < ChanceBetterValue
                ? StandardNewValue
                : BetterNewValue;
            var (randomRow, randomColumn) = empties[index];
            return new Element(randomRow, randomColumn, value);
        }

        public void MapperElements(ChangeElement mapper)
        {
            ForEach(element => SetValue(new Element(
                element.Row,
                element.Column,
                mapper(element)
                )
            ));
        }

        public void ForEach(ActionByElement mapper)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    mapper(new Element(row, column, GetValue(column, row)));
                }
            }
        }

        public bool Include(Element element)
        {
            return _elements[element.Row, element.Column] == element.Value;
        }

        private static Drop DropFactory(bool isIncreasing)
        {
            return isIncreasing
                ? new Drop(innerIndex => innerIndex - 1)
                : innerIndex => innerIndex + 1;
        }

        private GetValue GetValueFactory(bool isAlongRow)
        {
            return isAlongRow
                ? new GetValue((outerItem, innerItem) =>
                    new Element(outerItem, innerItem, _elements[outerItem, innerItem]))
                : (outerItem, innerItem) =>
                    new Element(innerItem, outerItem, _elements[innerItem, outerItem]);
        }

        private SetValue SetValueFactory(bool isAlongRow)
        {
            return isAlongRow
                ? new SetValue((outerItem, innerItem, value) =>
                    _elements[outerItem, innerItem] = value)
                : (outerItem, innerItem, value) =>
                    _elements[innerItem, outerItem] = value;
        }

        private static bool IsInnerCondition(int index, int innerStart, int innerEnd)
        {
            var minIndex = Math.Min(innerStart, innerEnd);
            var maxIndex = Math.Max(innerStart, innerEnd);

            return minIndex <= index && index <= maxIndex;
        }

        private int CalculateNewItem(int innerItem, Drop drop, int innerStart, int innerEnd, GetValue getValue, int outerItem)
        {
            var newInnerItem = innerItem;
            do
            {
                newInnerItem = drop(newInnerItem);
            } while (IsInnerCondition(newInnerItem, innerStart, innerEnd)
                     && getValue(outerItem, newInnerItem).Value == _baseValue);

            return newInnerItem;
        }
    }
}
