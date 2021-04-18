using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Core_2048
{

    public delegate int ChangeElement(Element element);
    public delegate void ActionByElement(Element element);
    public delegate int MergeElements(int newValue, int oldValue);
    public delegate int Drop(int index);
    public delegate Element GetValue(int outerItem, int innerItem);
    public delegate void SetValue(int outerItem, int innerItem, int value);

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
            _elements = new int[Height, Width];
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
            HasUpdated = false;
            UpdatingMap = new Dictionary<Element, Element>();

            var updateLoop = new UpdateLoop(isAlongRow, isIncreasing, Height, Width, _elements, _baseValue);
            var hasUpdating = updateLoop.Loop(_mergeElements);
            if (hasUpdating != null)
            {
                HasUpdated = true;
                UpdatingMap = hasUpdating;
            }
            else
            {
                UpdatingMap = new Dictionary<Element, Element>();
            }
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

        [CanBeNull]
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
            if (empties.Count == 0)
            {
                return null;
            }

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
                    mapper(new Element(row, column, GetValue(row, column)));
                }
            }
        }

        public bool Include(Element element)
        {
            return _elements[element.Row, element.Column] == element.Value;
        }
    }

}
