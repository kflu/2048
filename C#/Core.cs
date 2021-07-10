using System;
using System.Collections.Generic;

namespace Core_2048
{

    /// <summary>
    ///     A class for creating, managing and configuring the main logic of the game 2048
    /// </summary>
    /// <typeparam name="T">Value class of element on the board</typeparam>
    public partial class Core<T>
    {
        private readonly Board _board;
        private readonly IElementGenerator _elementGenerator;
        private readonly IValueBehavior _valueBehavior;

        /// <summary>
        ///     Invoke when call <see cref="Update">update</see> and have updated elements
        /// </summary>
        private Action<Dictionary<Element, Element>> _updated;

        /// <summary>
        ///     Main constructor
        /// </summary>
        /// <param name="board">Board with elements</param>
        /// <param name="elementGenerator"></param>
        /// <param name="valueBehavior"></param>
        /// <exception cref="ArgumentNullException">throw when any of the arguments is null</exception>
        public Core(Board board, IElementGenerator elementGenerator, IValueBehavior valueBehavior)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _elementGenerator = elementGenerator ?? throw new ArgumentNullException(nameof(elementGenerator));
            _valueBehavior = valueBehavior ?? throw new ArgumentNullException(nameof(valueBehavior));
        }

        public void AddUpdateListener(Action<Dictionary<Element, Element>> action)
        {
            _updated += action;
        }

        public void RemoveUpdateListener(Action<Dictionary<Element, Element>> action)
        {
            _updated -= action;
        }

        public void AddNew()
        {
            var element = _elementGenerator.GetNewElement(_board);
            if (element == null)
            {
                return;
            }

            _board.Set(element);
        }

        public void Update(bool isAlongRow, bool isIncreasing)
        {
            var changes = CalculateChanges(isAlongRow, isIncreasing);
            var updateMap = new Dictionary<Element, Element>();
            foreach (var changeElementAction in changes)
            {
                var prev = changeElementAction.Previous;
                var next = changeElementAction.Next;
                if (prev.Row == next.Row && prev.Column == next.Column)
                {
                    continue;
                }

                var baseValue = _valueBehavior.Instantiate(_valueBehavior.BaseValue, prev.Row, prev.Column);
                _board.Set(prev.Row, prev.Column, baseValue).Set(next);
                updateMap.Add(prev, next);
            }

            _updated?.Invoke(updateMap);
        }

        public void Render(Board.Mapper mapper)
        {
            _board.ForEach(mapper);
        }

        public IEnumerable<ChangeElementAction> CalculateChanges(bool isAlongRow, bool isIncreasing)
        {
            return new UpdateLoop(
                new UpdateBehavior(_board, isIncreasing, isAlongRow),
                _valueBehavior
            );
        }
    }

}
