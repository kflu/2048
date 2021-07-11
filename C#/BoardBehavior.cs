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
            return new UpdateLoop(_cellBehavior, _board, isAlongRow, isIncreasing);
        }

        public class ChangeElementAction
        {
            public Cell<T> Next;
            public Cell<T> Previous;
        }
    }

}
