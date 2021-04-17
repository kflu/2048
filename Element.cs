namespace Core_2048
{
    public class Element
    {
        public readonly int Row;
        public readonly int Column;
        public readonly int Value;

        public Element(int row, int column, int value)
        {
            Row = row;
            Column = column;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (typeof(Element) != obj.GetType()) return false;

            var otherElement = (Element) obj;

            var isEquals = otherElement.Column == Column
                           && otherElement.Row == Row
                           && otherElement.Value == Value;

            return isEquals;
        }

        protected bool Equals(Element other)
        {
            return Row == other.Row && Column == other.Column && Value == other.Value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Row;
                hashCode = (hashCode * 397) ^ Column;
                hashCode = (hashCode * 397) ^ Value;
                return hashCode;
            }
        }
    }
}
