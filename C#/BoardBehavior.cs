﻿using System;
using System.Collections.Generic;

namespace Core_2048
{

    public class BoardBehavior<T>
    {
        public Action<Dictionary<Cell<T>, Cell<T>>> Updated;

        public BoardBehavior(Board<T> board)
        {
            Board = board;
        }

        public Board<T> Board { get; set; }
        public UpdateLoop<T>.Merge Merge { get; set; }
        public UpdateLoop<T>.Predictor Predictor { get; set; }
        public T BaseValue { get; set; }
        public ICellGenerator<T> CellGenerator { get; set; }

        public void AddNew()
        {
            var element = CellGenerator.GetNewElement(Board);
            if (element == null)
            {
                return;
            }

            Board.Set(element);
        }

        public void Update(bool isAlongRow, bool isIncreasing)
        {
            var changes = CalculateChanges(isAlongRow, isIncreasing);
            var updateMap = new Dictionary<Cell<T>, Cell<T>>();
            foreach (var changeElementAction in changes)
            {
                var prev = changeElementAction.Previous;
                var next = changeElementAction.Next;
                Board.Set(prev.Row, prev.Column, BaseValue)
                    .Set(next);
                updateMap.Add(prev, next);
            }

            Updated?.Invoke(updateMap);
        }

        public IEnumerable<UpdateLoop<T>.ChangeElementAction> CalculateChanges(bool isAlongRow, bool isIncreasing)
        {
            var outerCount = isAlongRow ? Board.Height : Board.Width;
            var innerCount = isIncreasing ? Board.Width : Board.Height;

            var innerStart = isIncreasing ? 0 : innerCount - 1;
            var innerEnd = isIncreasing ? innerCount - 1 : 0;

            return new UpdateLoop<T>(
                BaseValue,
                DropFactory(isIncreasing),
                GetterFactory(isAlongRow),
                innerEnd,
                innerStart,
                index =>
                {
                    var minIndex = Math.Min(innerStart, innerEnd);
                    var maxIndex = Math.Max(innerStart, innerEnd);

                    return minIndex <= index && index <= maxIndex;
                },
                Merge,
                outerCount,
                Predictor,
                DropFactory(!isIncreasing)
            );
        }

        public UpdateLoop<T>.Drop DropFactory(bool isIncreasing)
        {
            return isIncreasing
                ? new UpdateLoop<T>.Drop(innerIndex => innerIndex - 1)
                : innerIndex => innerIndex + 1;
        }

        public UpdateLoop<T>.Get GetterFactory(bool isAlongRow)
        {
            return isAlongRow
                ? new UpdateLoop<T>.Get((outerItem, innerItem) => new Cell<T>
                {
                    Row = outerItem,
                    Column = innerItem,
                    Value = Board.Get(outerItem, innerItem)
                })
                : (outerItem, innerItem) => new Cell<T>
                {
                    Row = innerItem,
                    Column = outerItem,
                    Value = Board.Get(innerItem, outerItem)
                };
        }

        #region Builder

        public static CoreBuilder Builder()
        {
            return new CoreBuilder();
        }

        public class CoreBuilder
        {
            private Board<T> _board;
            private UpdateLoop<T>.Merge _merge;
            private UpdateLoop<T>.Predictor _predictor;
            private T _baseValue;
            private ICellGenerator<T> _cellGenerator;

            public CoreBuilder SetBoard(Board<T> board)
            {
                _board = board;

                return this;
            }

            public CoreBuilder SetMerge(UpdateLoop<T>.Merge merge)
            {
                _merge = merge;

                return this;
            }

            public CoreBuilder SetPredictor(UpdateLoop<T>.Predictor predictor)
            {
                _predictor = predictor;

                return this;
            }

            public CoreBuilder SetBaseValue(T baseValue)
            {
                _baseValue = baseValue;

                return this;
            }

            public CoreBuilder SetElementGenerator(ICellGenerator<T> cellGenerator)
            {
                _cellGenerator = cellGenerator;

                return this;
            }

            public BoardBehavior<T> Build()
            {
                return new BoardBehavior<T>(_board)
                {
                    Merge = _merge,
                    Predictor = _predictor,
                    BaseValue = _baseValue,
                    CellGenerator = _cellGenerator
                };
            }
        }

        #endregion
    }

}
