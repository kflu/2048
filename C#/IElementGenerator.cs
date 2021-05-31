using JetBrains.Annotations;

namespace Core_2048
{
    public interface IElementGenerator<T>
    {
        [CanBeNull] Element<T> GetNewElement(Board<T> board);
    }
}
