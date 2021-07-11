using System;
using System.Collections.Generic;

namespace Core_2048
{

    public partial class BoardBehavior<T>
    {
        private readonly Board<T> _board;
        private readonly ICellBehavior<T> _cellBehavior;
        private readonly ICellGenerator<T> _cellGenerator;

        private Action<Dictionary<Cell<T>, Cell<T>>> _updated;

        public BoardBehavior(Board<T> board, ICellGenerator<T> cellGenerator, ICellBehavior<T> cellBehavior,
            Action<Dictionary<Cell<T>, Cell<T>>> updated = null)
        {
            _updated = updated;
            _board = board;
            _cellGenerator = cellGenerator;
            _cellBehavior = cellBehavior;
        }

        public void AddUpdatedListener(Action<Dictionary<Cell<T>, Cell<T>>> action)
        {
            _updated += action;
        }

        public void RemoveUpdatedListener(Action<Dictionary<Cell<T>, Cell<T>>> action)
        {
            _updated -= action;
        }

        public void AddNew()
        {
            var element = _cellGenerator.GetNewElement(_board);
            if (element == null)
            {
                return;
            }

            _board.Set(element);
        }

        public void Update(bool isAlongRow, bool isIncreasing)
        {
            var changes = CalculateChanges(isAlongRow, isIncreasing);
            var updateMap = new Dictionary<Cell<T>, Cell<T>>();
            foreach (var changeElementAction in changes)
            {
                var prev = changeElementAction.Previous;
                var next = changeElementAction.Next;
                _board.Set(prev.Row, prev.Column, _cellBehavior.GetCellBaseValue())
                    .Set(next);
                updateMap.Add(prev, next);
            }

            _updated?.Invoke(updateMap);
        }

        public IEnumerable<ChangeElementAction> CalculateChanges(bool isAlongRow, bool isIncreasing)
        {
            var outerCount = isAlongRow ? _board.Height : _board.Width;
            var innerCount = isIncreasing ? _board.Width : _board.Height;

            var innerStart = isIncreasing ? 0 : innerCount - 1;
            var innerEnd = isIncreasing ? innerCount - 1 : 0;

            return new UpdateLoop(
                _cellBehavior,
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
                outerCount,
                DropFactory(!isIncreasing)
            );
        }

        private UpdateLoop.Drop DropFactory(bool isIncreasing)
        {
            return isIncreasing
                ? new UpdateLoop.Drop(innerIndex => innerIndex - 1)
                : innerIndex => innerIndex + 1;
        }

        private UpdateLoop.Get GetterFactory(bool isAlongRow)
        {
            return isAlongRow
                ? new UpdateLoop.Get((outerItem, innerItem) => new Cell<T>
                {
                    Row = outerItem,
                    Column = innerItem,
                    Value = _board.Get(outerItem, innerItem)
                })
                : (outerItem, innerItem) => new Cell<T>
                {
                    Row = innerItem,
                    Column = outerItem,
                    Value = _board.Get(innerItem, outerItem)
                };
        }

        public class ChangeElementAction
        {
            public Cell<T> Next;
            public Cell<T> Previous;
        }
    }

}
