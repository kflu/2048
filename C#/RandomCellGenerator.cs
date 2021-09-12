using System;
using System.Collections.Generic;
using System.Linq;

namespace Core_2048
{

    public class RandomCellGenerator<T> : ICellGenerator<T>
    {
        private readonly Dictionary<T, int> _pool = new Dictionary<T, int>();
        private readonly Random _random = new Random();
        private readonly Predicate<T> _emptyChecker;

        private int _allPercentage;

        public RandomCellGenerator(Predicate<T> emptyChecker)
        {
            _emptyChecker = emptyChecker;
        }

        public Cell<T> GetNewElement(Board<T> board)
        {
            var empties = BoardHelper<T>.GetEmpties(board, _emptyChecker).ToList();
            if (empties.Count == 0)
            {
                return null;
            }

            var index = _random.Next(0, empties.Count());
            var randomPosition = empties[index];

            var predicatePool = new Dictionary<Predicate<int>, T>();
            _pool.Aggregate(0, (percentage, pair) =>
            {
                var min = percentage;
                var max = percentage + pair.Value;
                predicatePool.Add(checkPercentage => min < checkPercentage && checkPercentage <= max, pair.Key);

                return max;
            });
            var resultPercentage = _random.Next(1, _allPercentage);
            var resultElements = from pair in predicatePool
                let predicate = pair.Key
                let element = pair.Value
                where predicate.Invoke(resultPercentage)
                select element;

            return new Cell<T>
            {
                Row = randomPosition.Row,
                Column = randomPosition.Column,
                Value = resultElements.First()
            };
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
    }

}
