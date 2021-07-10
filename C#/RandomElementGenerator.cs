using System;
using System.Collections.Generic;
using System.Linq;

namespace Core_2048
{

    public partial class Core<T>
    {
        public class RandomElementGenerator : IElementGenerator
        {
            private readonly Dictionary<T, int> _pool;
            private readonly IRandomizer _random;
            private readonly IValueBehavior _valueBehavior;

            private int _allPercentage;

            public RandomElementGenerator(IValueBehavior valueBehavior, IRandomizer random = null,
                Dictionary<T, int> pool = null)
            {
                _valueBehavior = valueBehavior ?? throw new ArgumentNullException(nameof(valueBehavior));
                _random = random ?? new BaseRandomizer();
                if (pool != null)
                {
                    foreach (var pair in pool)
                    {
                        _allPercentage += pair.Value;
                    }

                    _pool = pool;
                }
                else
                {
                    _pool = new Dictionary<T, int>();
                }
            }

            public Element GetNewElement(Board board)
            {
                var empties = BoardHelper.GetEmpties(board, _valueBehavior.IsBase).ToList();
                if (empties.Count == 0)
                {
                    return null;
                }

                var index = _random.Random(0, empties.Count);
                var randomPosition = empties[index];

                var resultElement = InstantiateRandomElementFromPool(randomPosition);

                return new Element
                {
                    Row = randomPosition.Row,
                    Column = randomPosition.Column,
                    Value = resultElement
                };
            }

            private T InstantiateRandomElementFromPool(Element position)
            {
                var predicatePool = new Dictionary<Predicate<int>, T>();
                _pool.Aggregate(0, (percentage, pair) =>
                {
                    var min = percentage;
                    var max = percentage + pair.Value;
                    predicatePool.Add(checkPercentage => min < checkPercentage && checkPercentage <= max, pair.Key);

                    return max;
                });
                var resultPercentage = _random.Random(1, _allPercentage);
                foreach (var pair in predicatePool.Where(pair => pair.Key.Invoke(resultPercentage)))
                {
                    return _valueBehavior.Instantiate(pair.Value, position.Row, position.Column);
                }

                throw new Exception(
                    $"Pool {_pool} have not object with percentage range that include the percentage {resultPercentage}");
            }

            public void AddToPool(T value, int percentage)
            {
                _pool.Add(value, percentage);
                _allPercentage += percentage;
            }

            public void AddToPool(T value)
            {
                var percentage = _pool.Count != 0 ? _allPercentage / _pool.Count : 1;
                AddToPool(value, percentage);
            }

            private class BaseRandomizer : IRandomizer
            {
                private readonly Random _random = new Random();

                public int Random(int min, int max)
                {
                    return _random.Next(min, max);
                }
            }
        }
    }

}
