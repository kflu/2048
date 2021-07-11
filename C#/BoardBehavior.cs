using System;
using System.Collections.Generic;

namespace Core_2048
{

    public class BoardBehavior<T>
    {
        public Action<Dictionary<Cell<T>, Cell<T>>> Updated;

        public BoardBehavior(Board<T> board, UpdateLoop<T>.Merge merge, UpdateLoop<T>.Predictor predictor, T baseValue,
            ICellGenerator<T> cellGenerator, Action<Dictionary<Cell<T>, Cell<T>>> updated = null)
        {
            Updated = updated;
            Board = board;
            Merge = merge;
            Predictor = predictor;
            BaseValue = baseValue;
            CellGenerator = cellGenerator;
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
    }

}
