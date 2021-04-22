2048
====

My take mini sdk libs for creating the 2048 game in C#. Suitable for any engine from console app to Unity game. Only
2048 game logic. A very simple customizable core for initialization game with params:

* canvas size
* base value, which means blank cells
* start score
* merge delegate
* standard and rare value for new cells
* chance of creating cells with rare value

## Easy to implements in your game:

1. Create a variable from Core class.
2. Set start value for any cell to init first playable cell in gameboard
3. In game loop, call UpdateElements method with arguments:
    * ***isAlongRow*** - true when movement **left** and **right** if render from up to down;
    * ***isIncreasing*** - true when movement **left** and **up** (if board render from **up** to **down**) or **down** (if board render from **down** to **up**);
4. Check properties HasUpdate to see if cells moved after ant action.
5. Call NewElement method when need add new value in random cell at board.
6. Create Render method in your engine. In Core class has methods for base iteration and there is updating map with movement info for easier implementation of interaction with external object.

Sample console project in ConsoleOut directory.

### Here's how it looks like:
![screenshot](doc/ConsoleOut.png "Console app for 2048")

### Also implementation in Unity:
![screenshot](doc/GameInUnity.gif "Unity app for 2048")
