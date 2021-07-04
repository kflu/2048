namespace Core_2048
{

    public partial class Core<T>
    {
        public interface IElementGenerator
        {
            Element GetNewElement(Board board);
        }
    }

}
