namespace Core_2048
{

    public class Element<T>
    {
        public int Column;
        public int Row;
        public T Value;

        #region Builder

        public static ElementBuilder Builder()
        {
            return new ElementBuilder();
        }

        public class ElementBuilder
        {
            private int _column;
            private int _row;
            private T _value;

            public ElementBuilder SetRow(int row)
            {
                _row = row;

                return this;
            }

            public ElementBuilder SetColumn(int column)
            {
                _column = column;

                return this;
            }

            public ElementBuilder SetValue(T value)
            {
                _value = value;

                return this;
            }

            public Element<T> Build()
            {
                var build = new Element<T>
                {
                    Row = _row,
                    Column = _column,
                    Value = _value
                };

                return build;
            }
        }

        #endregion
    }

    public class Element : Element<ulong> { }

}
