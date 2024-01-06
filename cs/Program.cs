class Program {
    const int SIZE = 8;
    static Ship[] SHIPS = { new Ship(2, 1, "destroyer"), new Ship(2, 2, "submarine"), new Ship(1, 3, "carrier") };
    const string SAVE_FILE = "game.bin";
    const bool SHOW_ENEMY_SHIPS = false;

    /// represents the state of one location in the grid
    enum Cell {
        Empty,
        Miss,
        Ship,
        Hit
    }

    /// represents a type of ship
    struct Ship {
        public int num;
        public int len;
        public string name;

        public Ship(int n, int l, string na) {
            num = n;
            len = l;
            name = na;
        }
    }

    /// show the menu to the user until q is entered
    static void Main() {
        while (true) {
            Console.WriteLine("- Battleboats -");
            Console.WriteLine("p - play a new game");
            Console.WriteLine("r - resume a game");
            Console.WriteLine("i - read instructions");
            Console.WriteLine("q - quit");

            string option = Prompt("enter menu option").ToLower();

            switch (option) {
                case "p":
                case "play":
                    Play(InputShips(), GenShips());
                    break;
                case "r":
                case "resume":
                    BinaryReader file = new BinaryReader(File.Open(SAVE_FILE, FileMode.Open));
                    Cell[,] playerGrid = ReadGrid(file);
                    Cell[,] pcGrid = ReadGrid(file);
                    file.Close();
                    Play(playerGrid, pcGrid);
                    break;
                case "i":
                case "read":
                case "instructions":
                    Instructions();
                    break;
                case "q":
                case "quit":
                    return;
                default:
                    Console.WriteLine("unknown option '" + option + "'");
                    break;
            }
        }
    }

    /// main gameplay loop
    static void Play(Cell[,] playerGrid, Cell[,] pcGrid) {
        Random rng = new Random();
        while (true) {
            // save
            BinaryWriter file = new BinaryWriter(File.Open(SAVE_FILE, FileMode.Create));
            WriteGrid(playerGrid, file);
            WriteGrid(pcGrid, file);
            file.Close();

            DisplayGrid(playerGrid, "fleet grid", (0, 0), true);
            DisplayGrid(pcGrid, "tracker grid", (2 * (SIZE + 2), SIZE + 2), false);

            // player turn
            (int, int) coord;
            while (true) {
                coord = ParseCoord(Prompt("enter target coord"));
                if (coord.Item1 < 0) {
                    Console.WriteLine("invalid coordinate");
                    continue;
                }
                if (Fire(ref pcGrid, coord, true)) {
                    break;
                }
            }
            if (!HasShips(pcGrid)) {
                Console.WriteLine("you won");
                return;
            }

            // computer turn
            Console.Write("computer's turn: ");
            while (true) {
                int x = rng.Next(0, SIZE);
                int y = rng.Next(0, SIZE);
                if (Fire(ref playerGrid, (y,x), false)) {
                    break;
                }
            }
            if (!HasShips(playerGrid)) {
                Console.WriteLine("you lost");
                return;
            }
        }
    }

    /// prompts the user to pick positions for ships
    static Cell[,] InputShips() {
        Console.WriteLine("place your ships");
        Cell[,] grid = new Cell[SIZE, SIZE];
        for (int i = 0; i < SHIPS.Length; i++) {
            int n = 0;
            while (n < SHIPS[i].num) {
                DisplayGrid(grid, "fleet grid", (0, 0), true);
                Console.WriteLine("placing ship " + (SHIPS[..i].Sum(s => s.num) + n + 1) + "/" + SHIPS.Sum(s => s.num) + ", length " + SHIPS[i].len);

                bool vert = false;
                if (SHIPS[i].len > 1) {
                    while (true) {
                        string dir = Prompt("enter ship direction (h/v)").ToLower();
                        if (dir == "h") {
                            break;
                        } else if (dir == "v") {
                            vert = true;
                            break;
                        } else {
                            Console.WriteLine("unknown direction");
                        }
                    }
                }

                (int, int) coord;
                while (true) {
                    coord = ParseCoord(Prompt("enter ship coord"));
                    if (coord.Item1 >= 0) {
                        break;
                    } else {
                        Console.WriteLine("invalid coordinate");
                    }
                }

                if (VerifyAndPlace(ref grid, coord, vert, SHIPS[i].len)) {
                    n++;
                } else {
                    Console.WriteLine("invalid placement");
                }
            }
        }
        return grid;
    }

    /// generate random ships
    static Cell[,] GenShips() {
        Random rng = new Random();
        Cell[,] grid = new Cell[SIZE, SIZE];
        for (int i = 0; i < SHIPS.Length; i++) {
            int n = 0;
            while (n < SHIPS[i].num) {
                if (VerifyAndPlace(ref grid, (rng.Next(0, SIZE), rng.Next(0, SIZE)), rng.Next() % 2 == 0, SHIPS[i].len)) {
                    n++;
                }
            }
        }
        return grid;
    }

    /// verifies the position of the given ship and returns true if it was placed in the grid
    static bool VerifyAndPlace(ref Cell[,] grid, (int, int) pos, bool vert, int size) {
        Cell[,] g = (Cell[,])grid.Clone();
        for (int i = 0; i < size; i++) {
            if (pos.Item1 < 0 || pos.Item1 >= SIZE || pos.Item2 < 0 || pos.Item2 >= SIZE) {
                return false;
            }

            if (g[pos.Item2, pos.Item1] != Cell.Empty) {
                return false;
            }
            g[pos.Item2, pos.Item1] = Cell.Ship;
            if (vert) {
                pos.Item2++;
            } else {
                pos.Item1++;
            }
        }

        grid = g;
        return true;
    }

    /// if the cell at pos is miss/hit already, returns false, otherwise replaces empty with miss and ship with hit
    /// always prints a message for hit/miss, and prints 'coordinate already picked' if player == true
    static bool Fire(ref Cell[,] grid, (int,int) pos, bool player) {
        switch (grid[pos.Item2, pos.Item1]) {
            case Cell.Empty:
                Console.WriteLine("miss " + FmtCoord(pos.Item1, pos.Item2));
                grid[pos.Item2, pos.Item1] = Cell.Miss;
                return true;
            case Cell.Ship:
                Console.WriteLine("hit " + FmtCoord(pos.Item1, pos.Item2));
                grid[pos.Item2, pos.Item1] = Cell.Hit;
                return true;
            default:
                if (player) {
                    Console.WriteLine("coordinate already picked");
                }
                return false;
        }
    }

    /// prints out the given grid at the given offset
    /// will only display ships if player == true, and misses if player == false
    static void DisplayGrid(Cell[,] grid, string title, (int, int) offset, bool player) {
        Console.CursorLeft += offset.Item1;
        Console.CursorTop -= offset.Item2;
        Console.WriteLine(title + ":");
        Console.CursorLeft += offset.Item1;
        Console.Write(" ");
        for (int n = 0; n < SIZE; n++) {
            Console.Write(" " + (char)('A' + n));
        }
        for (int y = 0; y < SIZE; y++) {
            Console.WriteLine();
            Console.CursorLeft += offset.Item1;
            Console.Write(y + 1);
            for (int x = 0; x < SIZE; x++) {
                switch (grid[y, x]) {
                    case Cell.Ship when player || SHOW_ENEMY_SHIPS:
                        Console.Write(" #");
                        break;
                    case Cell.Miss when !player:
                        Console.Write(" -");
                        break;
                    case Cell.Hit:
                        Console.Write(" X");
                        break;
                    default:
                        Console.Write(" ~");
                        break;
                }
            }
        }
        Console.WriteLine();
    }

    /// write the given grid to a file
    static void WriteGrid(Cell[,] grid, BinaryWriter file) {
        for (int y = 0; y < SIZE; y++) {
            for (int x = 0; x < SIZE; x++) {
                file.Write((Byte)grid[y,x]);
            }
        }
    }
    
    /// read a grid from the given file
    static Cell[,] ReadGrid(BinaryReader file) {
        Cell[,] grid = new Cell[SIZE, SIZE];
        for (int y = 0; y < SIZE; y++) {
            for (int x = 0; x < SIZE; x++) {
                grid[y,x] = (Cell)file.ReadByte();
            }
        }
        return grid;
    }

    /// parse the given string (case insensitive) as a coordinate (0-based) (B5 -> (1,4))
    /// verifies if the coordinate is within (SIZE, SIZE)
    /// returns (-1, -1) if the coordinate is invalid
    static (int, int) ParseCoord(string coord) {
        coord = coord.ToUpper();
        if (coord.Length == 2 && (coord[0] >= 'A' && coord[0] <= 'A' + SIZE - 1) && (coord[1] >= '1' && coord[1] <= '1' + SIZE - 1)) {
            return (coord[0] - 'A', coord[1] - '1');
        }
        return (-1, -1);
    }

    /// returns a string representing the given coordinate ((2,3) -> D4)
    static string FmtCoord(int x, int y) {
        return (char)('A' + x) + (y + 1).ToString();
    }

    /// returns true if there are ships in the given grid
    static bool HasShips(Cell[,] grid) {
        for (int y = 0; y < SIZE; y++) {
            for (int x = 0; x < SIZE; x++) {
                if (grid[y, x] == Cell.Ship) {
                    return true;
                }
            }
        }
        return false;
    }

    /// display the given prompt and read 1 line from the console
    static string Prompt(string p) {
        Console.Write(p + ": ");
        return Console.ReadLine();
    }

    /// prints out the instructions
    static void Instructions() {
        Console.WriteLine("instructions:");
        Console.WriteLine(" setup your fleet grid by entering coordinates (C3, E5, ...)");
        Console.WriteLine(" choose the direction of longer ships by entering h/v for horizontal/vertical.");
        Console.WriteLine(" pick coordinates on the target tracker to attack");
        Console.WriteLine(" the grid will either show hit (X) or miss (-).");
        Console.WriteLine(" the first player to destroy all the enemy ships wins.");
        Console.WriteLine(" ship types:");
        for (int i = 0; i < SHIPS.Length; i++) {
            Console.WriteLine(" " + SHIPS[i].num + " x " + SHIPS[i].name + " (" + SHIPS[i].len + " cell(s))");
        }
    }
}
