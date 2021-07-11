using System;
using System.Collections.Generic;

namespace Core_2048
{

    public class BoardBehavior<T>
    {
        public Action<Dictionary<Element<T>, Element<T>>> Updated;

        public BoardBehavior(Board<T> board)
        {
            Board = board;
        }

        public Board<T> Board { get; set; }
        public UpdateLoop<T>.Merge Merge { get; set; }
        public UpdateLoop<T>.Predictor Predictor { get; set; }
        public T BaseValue { get; set; }
        public IElementGenerator<T> ElementGenerator { get; set; }

        public void AddNew()
        {
            var element = ElementGenerator.GetNewElement(Board);
            if (element == null)
            {
                return;
            }

            Board.Set(element);
        }

        public void Update(bool isAlongRow, bool isIncreasing)
        {
            var changes = CalculateChanges(isAlongRow, isIncreasing);
            var updateMap = new Dictionary<Element<T>, Element<T>>();
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

            return UpdateLoop<T>.Builder()
                .SetDrop(DropFactory(isIncreasing))
                .SetReverseDrop(DropFactory(!isIncreasing))
                .SetGetter(GetterFactory(isAlongRow))
                .SetMerge(Merge)
                .SetPredictor(Predictor)
                .SetBaseValue(BaseValue)
                .SetOuterCount(outerCount)
                .SetInnerCondition(innerStart, innerEnd)
                .Build();
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
                ? new UpdateLoop<T>.Get((outerItem, innerItem) => Element<T>.Builder()
                    .SetRow(outerItem)
                    .SetColumn(innerItem)
                    .SetValue(Board.Get(outerItem, innerItem))
                    .Build())
                : (outerItem, innerItem) => Element<T>.Builder()
                    .SetRow(innerItem)
                    .SetColumn(outerItem)
                    .SetValue(Board.Get(innerItem, outerItem))
                    .Build();
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
            private IElementGenerator<T> _elementGenerator;

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

            public CoreBuilder SetElementGenerator(IElementGenerator<T> elementGenerator)
            {
                _elementGenerator = elementGenerator;

                return this;
            }

            public BoardBehavior<T> Build()
            {
                return new BoardBehavior<T>(_board)
                {
                    Merge = _merge,
                    Predictor = _predictor,
                    BaseValue = _baseValue,
                    ElementGenerator = _elementGenerator
                };
            }
        }

        #endregion
    }

}
