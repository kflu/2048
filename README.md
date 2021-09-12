2048  [![Nuget](https://img.shields.io/nuget/v/Core_2048)](https://www.nuget.org/packages/Core_2048) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/VladShyrokyi/2048-1)](https://github.com/VladShyrokyi/2048-1)
====

My take mini sdk libs for creating the 2048 game in C#. Suitable for any engine from console app to Unity game. Only
2048 game logic. A very simple customizable core for initialization game with params:

* canvas size
* base value, which means blank cells
* configuring merging cells value
* configuring predicate cells value
* customizable amount of value for new cells with customizable chance of creation for each individual case
* generic for the cells value (Convenient for use in game engines)

## Easy to implements in your game:

1. Create a game `Board` by your generic type with **height**, **width** and **initialized function**.
2. Create a generator of random cells `RandomCellGenerator`, set the check to an empty cell and add a list of cells to the pool to generate with a specified percentage probability (Or you can create your own generator by implementing `IElementGenerator`).
3. Create an implementation of the `ICellBehavior` interface to define base cells and behavior for merging cells.
4. Create `BoardBehavior` with `Board`, class implementing `ICellBehavior`, and `CellGenerator`.
5. Call `AddNew` for generating and add new element on board.
6. In the game loop, call the `Update` with arguments:
    * `isAlongRow` - true when movement **left** and **right** if render from up to down;
    * `isIncreasing` - true when movement **left** and **up** (if board render from **up** to **down**) or **down** (if board render from **down** to **up**);
7. Add listeners on `Updated` action to see if cells moved after ant action.
8. Call `AddNew` method when need add new value at board.
9. Create `Render` method in your engine. In Core class has methods for base iteration and there is updating map with movement info for easier implementation of interaction with external object.

Sample console project in ConsoleOut directory.

### Example code:
```csharp
var board = new Board<ulong>(4, 4, () => 0);
var elementGenerator = new RandomCellGenerator<ulong>(element => element == 0);
elementGenerator.AddToPool(2, 95);
elementGenerator.AddToPool(4, 5);
var app = new BoardBehavior<ulong>(board, elementGenerator, new BaseCellBehavior());
app.AddNew();
app.AddUpdatedListener(elements =>
{
    app.AddNew();
    Render(board);
});
Render(board);
while (true)
{
    var direction = Input();
    if (direction == null)
    {
        continue;
    }

    var isAlongRow = direction == Direction.Left || direction == Direction.Right;
    var isIncreasing = direction == Direction.Left || direction == Direction.Up;
    app.Update(isAlongRow, isIncreasing);
}
```

### Here's how it looks like:

![screenshot](https://raw.githubusercontent.com/VladShyrokyi/2048-1/master/doc/ConsoleOut.png "Console app for 2048")

### Also implementation in Unity:

![screenshot](https://raw.githubusercontent.com/VladShyrokyi/2048-1/master/doc/GameInUnity.gif "Unity app for 2048")
