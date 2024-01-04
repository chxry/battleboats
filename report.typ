#set par(justify: true)
#set page(numbering: "1")
#set text(hyphenate: false)
#set heading(numbering: "1.")
#set text(12pt)
#show link: underline

#let fn(name, params, returns) = [
  #text(13pt, weight: 700, name) \
  Parameters: #raw(params) \
  Returns: #raw(returns) \
]

#let test(path) = block(fill: luma(230), inset: 8pt, radius: 4pt, width: 100%, raw(read(path)))

#page(numbering: none, [
  #v(2fr)
  #align(center, [
    #text(26pt, weight: 700, [Battleboats Project])
    #v(0.1fr)
    #text(16pt, [Alexander Tahiri])
  ])
  #v(2fr)
])

#outline(indent: true)
#pagebreak()

= Objectives
1. The program will present the user with a menu that allows the user to play a new game, resume a game, read instructions for the game or quit the game. The program will verify that the given menu option is valid. The program should accept an abbreviated version of the option (`'p'`), or the full version (`'play'`)
2. When the user selects the menu option to play a new game, the user will be prompted for 5 boat directions and  coordinates. After entering each coordinate, the program will display the new fleet grid.
3. The program should be able to generate the computer's fleet grid by randomly selecting 5 directions and coordinates within the grid.
4. The program should verify that the player and computer generated boats do not overlap, and are within the grid.
5. The program should display the user's fleet grid and target tracker after each turn.
6. Each turn, the program should first prompt the user for their target coordinates. The program should verify these coordinates are within the grid and have not been used before. If there is a boat in the computer's fleet grid at the given coordinates, a hit should be shown at that position on the user's target tracker, otherwise a miss should be shown.
7. After the user's turn, the program should randomly select a coordinate within the grid that the computer has not used before. If there is a boat in the user's fleet grid, it should be replaced with a hit.
8. The program should repeat the user and computer turns until all the boats in either of the fleet grids have been hit. The winner (the player that still has boats remaining), should be displayed to the user, and the program should return to the menu.
9. After each turn the program should save the current state of the game to a file.
10. When the user selects the menu option to resume a game, the state of the game should be loaded from a file and start the current player's turn.
11. When the user selects the menu option to read instructions, the program should display a clear explanation of the game and return to the menu.

= Documented Design
== Data Structures
The player and computer grids are stored using a 2D arrays of the enum `Cell`, which has 4 states:
- `Empty` - There is no ship at this position.
- `Miss` - The enemy has fired at this position, but there is no ship.
- `Ship` - There is an intact ship at this position.
- `Hit` - The enemy has hit a ship at this position.
The types of ships are stored in an array of the struct `Ship`, which has the following members:
- `int num` - The number of this type of ship.
- `int len` - The length of this ship.
- `string name` - The name of this ship. Used for instructions menu.
== Constants
- `int SIZE` - The size of the grids (default 8). Values above 9 will not be rendered correctly, however this is not in the project scenario.
- `Ship[] SHIPS` - An array of the types of ships.
- `string SAVE_FILE` - A path to the game's save file (default "game.bin").
- `bool SHOW_ENEMY_SHIPS` - If the program should display enemy ships on the target tracker (default false). Used for debugging.
== Functions
#fn("Main", "None", "None")
Entry point. Shows the menu to the user until 'q' is entered.

#fn("Play", "Cell[,] playerGrid, Cell[,] pcGrid", "None")
Main gameplay loop. Expects filled out grids.

#fn("InputShips", "None", "Cell[,]")
Prompts the user to input their ships, returns their grid.

#fn("GenShips", "None", "Cell[,]")
Generates a grid with random ships.

#fn("VerifyAndPlace", "Cell[,] grid, (int, int) pos, bool vert, int size", "bool")
Verifies if a ship at position `pos`, length `size`, placed vertically if `vert == true` would fall within the size of the grid and not intersect and ships.
Places the ship and returns true if the position is valid.

#fn("DisplayGrid", "Cell[,] grid, string title, (int, int) offset, bool player", "None")
Prints out the given grid with the given offset relative to the current cursor.
Will only display ships if `player == true`, and misses if `player == false`.

#fn("WriteGrid", "Cell[,] grid, BinaryWriter file", "None")
Write the given grid to a file.

#fn("ReadGrid", "BinaryReader file", "Cell[,]")
Read a grid from the given file.

#fn("ParseCoord", "string coord", "(int,int)")
Parse the given string (case insensitive) as a coordinate (0-based) (B5 -> (1,4)).
Verifies if the coordinate is within (SIZE, SIZE).
Returns (-1, -1) if the coordinate is invalid.

#fn("FmtCoord", "int x, int y", "string")
Returns a string representing the given coordinate ((2,3) -> D4).

#fn("HasShips", "Cell[,] grid", "bool")
Returns true if there are ships in the given grid.

#fn("Prompt", "string", "string")
Display the given prompt and read 1 line from the console.

#fn("Instructions", "None", "None")
Prints out instructions for the game.

= Technical Solution
The C\# source code for the project is availible on #link("https://github.com/chxry/battleboats", [GitHub]).
The `Program.cs` file is also attached to the Teams assignment.

= Testing
== Menu
#test("tests/menu.txt")
== Inputing ships
#test("tests/input.txt")
== Resuming
#test("tests/resume.txt")
== Win condition
#test("tests/win.txt")


= Evaluation
Overall, my project was successful and met all of the criteria, including the extensions. If I were to revisit the project, I would make the following improvements: 
- Computer targeting could consider the previous hits, attempting adjacent cells instead of complete randomness.
- The user interface could be made more visually appealing with colours and ASCII art, or even a GUI.
- The error handling on file loading on file loading could be improved.
