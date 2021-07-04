namespace Core_2048.Implementation
{

    public class Core : Core<ulong>
    {
        public Core(Board board) : base(board) { }

        public new class Board : Core<ulong>.Board
        {
            public Board(int height, int width, ulong initValue) : base(height, width, () => initValue) { }
        }
    }

}
