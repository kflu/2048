namespace Core_2048
{

    public interface ICellBehavior<T>
    {
        bool IsBaseCell(Cell<T> cell);
        bool IsMergeCells(Cell<T> previous, Cell<T> next);
        T MergeCells(Cell<T> previous, Cell<T> next);
        T GetCellBaseValue();
    }

}
