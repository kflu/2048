using System;
using System.Collections.Generic;

namespace Core_2048
{

    public partial class Core<T>
    {
        private readonly Board _board;
        public Action<Dictionary<Element, Element>> Updated;

        public Core(Board board)
        {
            _board = board;
        }

        public UpdateLoop.Merge Merge { get; set; }
        public UpdateLoop.Predictor Predictor { get; set; }
        public T BaseValue { get; set; }
        public IElementGenerator ElementGenerator { get; set; }

        public void AddNew()
        {
            var element = ElementGenerator.GetNewElement(_board);
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
                _board.Set(prev.Row, prev.Column, BaseValue)
                    .Set(next);
                updateMap.Add(prev, next);
            }

            Updated?.Invoke(updateMap);
        }

        public void Render(Board.Mapper mapper)
        {
            _board.ForEach(mapper);
        }

        public IEnumerable<UpdateLoop.ChangeElementAction> CalculateChanges(bool isAlongRow, bool isIncreasing)
        {
            var outerCount = isAlongRow ? _board.Height : _board.Width;
            var innerCount = isIncreasing ? _board.Width : _board.Height;

            var innerStart = isIncreasing ? 0 : innerCount - 1;
            var innerEnd = isIncreasing ? innerCount - 1 : 0;

            return UpdateLoop.Builder()
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

        public UpdateLoop.Drop DropFactory(bool isIncreasing)
        {
            return isIncreasing
                ? new UpdateLoop.Drop(innerIndex => innerIndex - 1)
                : innerIndex => innerIndex + 1;
        }

        public UpdateLoop.Get GetterFactory(bool isAlongRow)
        {
            return isAlongRow
                ? new UpdateLoop.Get((outerItem, innerItem) => new Element
                    {
                        Row = outerItem,
                        Column = innerItem,
                        Value = _board.Get(outerItem, innerItem)
                    }
                )
                : (outerItem, innerItem) => new Element
                {
                    Row = innerItem,
                    Column = outerItem,
                    Value = _board.Get(innerItem, outerItem)
                };
        }
    }

}
