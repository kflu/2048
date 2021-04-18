using System;

namespace Core_2048
{

    public static class Utils
    {
        public static Drop DropFactory(bool isIncreasing)
        {
            return isIncreasing
                ? new Drop(innerIndex => innerIndex - 1)
                : innerIndex => innerIndex + 1;
        }

        public static GetValue GetValueFactory(bool isAlongRow, int[,] elements)
        {
            return isAlongRow
                ? new GetValue((outerItem, innerItem) =>
                                   new Element(outerItem, innerItem, elements[outerItem, innerItem]))
                : (outerItem, innerItem) =>
                    new Element(outerItem, innerItem, elements[innerItem, outerItem]);
        }

        public static SetValue SetValueFactory(bool isAlongRow, int[,] elements)
        {
            return isAlongRow
                ? new SetValue((outerItem, innerItem, value) =>
                    elements[outerItem, innerItem] = value)
                : (outerItem, innerItem, value) =>
                    elements[innerItem, outerItem] = value;
        }

        public static bool IsInnerCondition(int index, int innerStart, int innerEnd)
        {
            var minIndex = Math.Min(innerStart, innerEnd);
            var maxIndex = Math.Max(innerStart, innerEnd);

            return minIndex <= index && index <= maxIndex;
        }
    }

}
