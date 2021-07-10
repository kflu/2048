using System;
using System.Collections;
using System.Collections.Generic;

namespace Core_2048
{

    public partial class Core<T>
    {
        internal class UpdateLoop : IEnumerable<ChangeElementAction>
        {
            private readonly UpdateBehavior _updateBehavior;
            private readonly IValueBehavior _valueBehavior;

            public UpdateLoop(UpdateBehavior updateBehavior, IValueBehavior valueBehavior)
            {
                _updateBehavior = updateBehavior ?? throw new ArgumentNullException(nameof(updateBehavior));
                _valueBehavior = valueBehavior ?? throw new ArgumentNullException(nameof(valueBehavior));
            }

            public IEnumerator<ChangeElementAction> GetEnumerator()
            {
                for (var outerItem = 0; outerItem < _updateBehavior.OuterCount; outerItem++)
                {
                    for (var innerItem = _updateBehavior.InnerStart;
                         _updateBehavior.IsInnerCondition(innerItem);
                         innerItem = _updateBehavior.ReverseDrop(innerItem))
                    {
                        if (_valueBehavior.IsBase(_updateBehavior.Get(outerItem, innerItem).Value))
                        {
                            continue;
                        }

                        var newInnerItem = CalculateNewItem(innerItem, outerItem);
                        var isMerge = _updateBehavior.IsInnerCondition(newInnerItem) && _valueBehavior.IsMerge(
                            _updateBehavior.Get(outerItem, newInnerItem).Value,
                            _updateBehavior.Get(outerItem, innerItem).Value
                        );

                        yield return isMerge
                            ? ExecuteWithMerge(outerItem, innerItem, newInnerItem)
                            : ExecuteWithoutMerge(outerItem, innerItem, newInnerItem);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private ChangeElementAction ExecuteWithMerge(int outerItem, int innerItem, int newInnerItem)
            {
                var previous = _updateBehavior.Get(outerItem, innerItem);
                var next = _updateBehavior.Get(outerItem, newInnerItem);

                next = new Element
                {
                    Row = next.Row,
                    Column = next.Column,
                    Value = _valueBehavior.Merge(previous.Value, next.Value)
                };

                return new ChangeElementAction
                {
                    Previous = previous,
                    Next = next
                };
            }

            private ChangeElementAction ExecuteWithoutMerge(int outerItem, int innerItem, int newInnerItem)
            {
                newInnerItem = _updateBehavior.ReverseDrop(newInnerItem);
                var previous = _updateBehavior.Get(outerItem, innerItem);
                var next = _updateBehavior.Get(outerItem, newInnerItem);

                next = new Element
                {
                    Row = next.Row,
                    Column = next.Column,
                    Value = previous.Value
                };

                return new ChangeElementAction
                {
                    Previous = previous,
                    Next = next
                };
            }

            private int CalculateNewItem(int innerItem, int outerItem)
            {
                var newInnerItem = innerItem;
                do
                {
                    newInnerItem = _updateBehavior.Drop(newInnerItem);
                } while (_updateBehavior.IsInnerCondition(newInnerItem) &&
                         _valueBehavior.IsBase(_updateBehavior.Get(outerItem, newInnerItem).Value));

                return newInnerItem;
            }
        }
    }

}
