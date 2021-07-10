namespace Core_2048
{

    public partial class Core<T>
    {
        /// <summary>
        ///     Interface for defining interaction with element values
        /// </summary>
        public interface IValueBehavior
        {
            /// <summary>
            ///     The value for setting empty elements on a board.
            /// </summary>
            T BaseValue { get; }

            /// <summary>
            ///     Check value is a base value. Example, is 0.
            /// </summary>
            /// <param name="value">Value from element on a board</param>
            /// <returns>If true, value will not be to change</returns>
            /// <seealso cref="UpdateLoop" />
            bool IsBase(T value);

            /// <summary>
            ///     Create a new value based on the value from the previous element and the next.
            /// </summary>
            /// <param name="previous">Value from previous row, column element</param>
            /// <param name="next">Value from next row, column element</param>
            /// <returns>Value for next element</returns>
            T Merge(T previous, T next);

            /// <summary>
            ///     Determine whether to merge a value from previous element to another element.
            /// </summary>
            /// <param name="previous">Value from element on a board</param>
            /// <param name="next">Value from element on a board</param>
            /// <returns>If true, value will be <see cref="Merge">merge</see></returns>
            bool IsMerge(T previous, T next);

            /// <summary>
            ///     Instantiate value from another values for setting element with new class of value.
            ///     For creating elements with value of custom class.
            /// </summary>
            /// <param name="prefab">Value not from element</param>
            /// <param name="row">Index in outer array dimensions</param>
            /// <param name="column">Index in inner array dimensions</param>
            /// <seealso cref="RandomElementGenerator" />
            /// <returns>Value for setting in element with row and column</returns>
            T Instantiate(T prefab, int row, int column);
        }
    }

}
