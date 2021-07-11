
namespace Core_2048
{

    public interface IElementGenerator<T>
    {
        Element<T> GetNewElement(Board<T> board);
    }

}
