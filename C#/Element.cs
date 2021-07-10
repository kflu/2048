namespace Core_2048
{

    public partial class Core<T>
    {
        /// <summary>
        ///     Container for value and indexes in inner/outer dimensions
        /// </summary>
        public class Element
        {
            /// <summary>
            ///     Index in inner array dimensions
            /// </summary>
            public int Column;

            /// <summary>
            ///     Index in outer array dimensions
            /// </summary>
            public int Row;

            /// <summary>
            ///     Value on array
            /// </summary>
            public T Value;
        }
    }

}
