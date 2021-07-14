
namespace Core_2048
{

    public interface ICellGenerator<T>
    {
        Cell<T> GetNewElement(Board<T> board);
    }

}
