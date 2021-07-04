using System;
using System.Collections.Generic;
using System.Linq;

namespace Core_2048
{

    public partial class Core<T>
    {
        public static class BoardHelper
        {
            public static IEnumerable<Element> GetEmpties(Board board, Predicate<T> emptyChecker)
            {
                return board.Where(value => emptyChecker.Invoke(value.Value));
            }
        }
    }

}
