#set par(justify: true)
#set page(numbering: "1")
#set text(hyphenate: false)
#set heading(numbering: "1.")
#set text(12pt)

#page(numbering: none, [
  #v(2fr)
  #align(center, [
    #text(24pt, weight: 700, [Battleboats Project])
    #v(0.1fr)
    #text(14pt, [Alexander Tahiri])
  ])
  #v(2fr)
])

#outline()
#pagebreak()

= Introduction

= Objectives
1. The program will present the user with a menu that allows the user to play a new game, resume a game, read instructions for the game or quit the game. The program will verify that the given menu option is valid. The program should accept an abbrevated version of the option (`'p'`), or the full version (`'play'`)
2. When the user selects the menu option to play a new game, the user will be prompted for 5 boat directions and  coordinates. After entering each coordinate, the program will display the new fleet grid.
3. The program should be able to generate the computer's fleet grid by randomly selecting 5 directions and coordinates within the grid.
4. The program should verify that the player and computer generated boats do not overlap, and are within the grid.
5. The program should display the user's fleet grid and target tracker after each turn.
6. Each turn, the program should first prompt the user for their target coordinates. The program should verify these coordinates are within the grid and have not been used before. If there is a boat in the computer's fleet grid at the given coordinates, a hit should be shown at that position on the user's target tracker, otherwise a miss should be shown.
7. After the user's turn, the program should randomly select a coordinate within the grid that the computer not used before. If there is a boat in the user's fleet grid, it should be replaced with a hit.
8. The program should repeat the user and computer turns until all the boats in either of the fleet grid's have been hit. The winner (the player that still has boats remaining), should be displayed to the user, and the program should return to the menu.
9. After each turn the program should save the current state of the game to a file.
10. When the user selects the menu option to resume a game, the state of the game should be loaded from a file and start the current player's turn.
11. When the user selects the menu option to read instructions, the program should display a clear explanation of the game and return to the menu.

= Documented Design

= Technical Solution

= Testing

= Evaluation
