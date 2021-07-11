
namespace Core_2048
{

    public interface IElementGenerator<T>
    {
        Cell<T> GetNewElement(Board<T> board);
    }

}
