class Program {
    const int SIZE = 8;
    static Ship[] SHIPS = { new Ship(2, 1, "destroyer"), new Ship(2, 2, "submarine"), new Ship(1, 3, "carrier") };
    const string SAVE_FILE = "game.bin";
    const bool SHOW_ENEMY_SHIPS = false;

    enum Cell {
        Empty,
        Miss,
        Ship,
        Hit
    }

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
                    Play(InputShips(), GenShips());
                    break;
                case "r":
                    BinaryReader file = new BinaryReader(File.Open(SAVE_FILE, FileMode.Open));
                    Cell[,] playerGrid = ReadGrid(file);
                    Cell[,] pcGrid = ReadGrid(file);
                    file.Close();
                    Play(playerGrid, pcGrid);
                    break;
                case "i":
                    Instructions();
                    break;
                case "q":
                    return;
                default:
                    Console.WriteLine("unknown option '" + option + "'");
                    break;
            }
        }
    }

    static void Play(Cell[,] playerGrid, Cell[,] pcGrid) {
        Random rng = new Random();
        bool player_won;
        while (true) {
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
                switch (pcGrid[coord.Item2, coord.Item1]) {
                    case Cell.Miss:
                    case Cell.Hit:
                        Console.WriteLine("coordinate already picked");
                        continue;
                    case Cell.Empty:
                        Console.WriteLine("miss");
                        pcGrid[coord.Item2, coord.Item1] = Cell.Miss;
                        break;
                    case Cell.Ship:
                        Console.WriteLine("hit");
                        pcGrid[coord.Item2, coord.Item1] = Cell.Hit;
                        break;
                }
                break;
            }
            if (!HasShips(pcGrid)) {
                player_won = true;
                break;
            }

            // computer turn
            Console.Write("computer's turn: ");
            while (true) {
                int x = rng.Next(0, SIZE);
                int y = rng.Next(0, SIZE);
                Console.WriteLine(FmtCoord(x, y));
                switch (playerGrid[y, x]) {
                    case Cell.Miss:
                    case Cell.Hit:
                        continue;
                    case Cell.Empty:
                        playerGrid[y, x] = Cell.Miss;
                        break;
                    case Cell.Ship:
                        playerGrid[y, x] = Cell.Hit;
                        break;
                }
                break;
            }
            if (!HasShips(playerGrid)) {
                player_won = false;
                break;
            }
        }
        if (player_won) {
            Console.WriteLine("you won");
        } else {
            Console.WriteLine("you lost");
        }
    }

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
                            vert = false;
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

    static void WriteGrid(Cell[,] grid, BinaryWriter file) {
        for (int y = 0; y < SIZE; y++) {
            for (int x = 0; x < SIZE; x++) {
                file.Write((Byte)grid[y,x]);
            }
        }
    }

    static Cell[,] ReadGrid(BinaryReader file) {
        Cell[,] grid = new Cell[SIZE, SIZE];
        for (int y = 0; y < SIZE; y++) {
            for (int x = 0; x < SIZE; x++) {
                grid[y,x] = (Cell)file.ReadByte();
            }
        }
        return grid;
    }

    static (int, int) ParseCoord(string coord) {
        coord = coord.ToUpper();
        if (coord.Length == 2 && (coord[0] >= 'A' && coord[0] <= 'A' + SIZE - 1) && (coord[1] >= '1' && coord[1] <= '1' + SIZE - 1)) {
            return (coord[0] - 'A', coord[1] - '1');
        }
        return (-1, -1);
    }

    static string FmtCoord(int x, int y) {
        return (char)('A' + x) + (y + 1).ToString();
    }

    static string Prompt(string p) {
        Console.Write(p + ": ");
        return Console.ReadLine();
    }

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
