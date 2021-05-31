using System;
using System.Collections.Generic;
using System.Linq;

namespace Core_2048
{
    public static class BoardHelper<T>
    {
        public static IEnumerable<Element<T>> GetEmpties(Board<T> board, Predicate<T> emptyChecker)
        {
            return board.Where(value => emptyChecker.Invoke(value.Value));
        }
    }
}
