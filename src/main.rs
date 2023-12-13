#![feature(generic_arg_infer)]
use std::io;
use std::io::Write;
use nanorand::{Rng, WyRand};

type Error = Box<dyn std::error::Error>;

const SIZE: usize = 8;
const SHIPS: usize = 5;
const DBG_SHOW_ENEMY_SHIPS: bool = false;

/// show the menu to the user until q/quit is entered
fn main() -> Result<(), Error> {
  loop {
    println!("- Battleboats -");
    println!("p - play a new game");
    println!("r - resume a game");
    println!("i - read instructions");
    println!("q - quit");
    match &*prompt("enter menu option")?.to_lowercase() {
      "p" | "play" => play()?,
      "r" | "resume" => println!("resume"),
      "i" | "read" => {
        println!("instructions: ");
        println!("...");
      }
      "q" | "quit" => return Ok(()),
      s => println!("unknown option '{}'", s),
    }
  }
}

type Grid = [[Cell; SIZE as _]; SIZE as _];

#[derive(Copy, Clone, PartialEq)]
enum Cell {
  Empty,
  Miss,
  Ship,
  Hit,
}

/// fill fleet grids and main gameplay loop
fn play() -> Result<(), Error> {
  let mut rng = WyRand::new();
  println!("place your ships");
  let mut player_grid = [[Cell::Empty; _]; _];
  let mut n = 1;
  while n <= SHIPS {
    display_grid(&player_grid, "fleet grid", (0, 0), true);
    match parse_coord(&prompt(&format!("enter ship coord ({}/{})", n, SHIPS))?) {
      Ok(c) => {
        if player_grid[c.1][c.0] == Cell::Empty {
          player_grid[c.1][c.0] = Cell::Ship;
          n += 1;
        } else {
          println!("ship position already occupied");
        }
      }
      Err(e) => println!("{}", e),
    }
  }

  let mut pc_grid = [[Cell::Empty; _]; _];
  let mut n = 1;
  while n <= SHIPS {
    let x = rng.generate_range(0..SIZE);
    let y = rng.generate_range(0..SIZE);
    if pc_grid[y][x] == Cell::Empty {
      pc_grid[y][x] = Cell::Ship;
      n += 1;
    }
  }

  // add win condition
  loop {
    display_grid(&player_grid, "fleet grid", (0, 0), true);
    display_grid(
      &pc_grid,
      "tracker grid",
      ((SIZE + 2) * 2, SIZE + 3),
      DBG_SHOW_ENEMY_SHIPS,
    );
    loop {
      match parse_coord(&prompt("enter target coord")?) {
        Ok(c) => match pc_grid[c.1][c.0] {
          Cell::Miss | Cell::Hit => println!("coordinate already picked"),
          Cell::Empty => {
            println!("Miss");
            pc_grid[c.1][c.0] = Cell::Miss;
            break;
          }
          Cell::Ship => {
            println!("Hit");
            pc_grid[c.1][c.0] = Cell::Hit;
            break;
          }
        },
        Err(e) => println!("{}", e),
      }
    }

    // computer turn
  }
}

/// parse the given string (case insensitive) as a coordinate (0-based) (B5 -> (1,4))
/// verifies if the coordinate is within the SIZE, SIZE
fn parse_coord(c: &str) -> Result<(usize, usize), &str> {
  let c = c.to_uppercase().into_bytes();
  if c.len() == 2 {
    if (b'A'..=b'A' + SIZE as u8).contains(&c[0]) && (b'1'..b'1' + SIZE as u8).contains(&c[1]) {
      Ok(((c[0] - b'A') as _, (c[1] - b'1') as _))
    } else {
      Err("coordinate out of bounds")
    }
  } else {
    Err("expected 2 characters")
  }
}

/// prints out the given grid at the given offset
/// will only display ships if player == true, and misses if player == false
fn display_grid(grid: &Grid, title: &str, offset: (usize, usize), player: bool) {
  println!("\x1b[{}C\x1b[{}A\x1b[1B{}:", offset.0, offset.1, title);
  print!("\x1b[{}C ", offset.0);
  for n in 0..SIZE {
    print!(" {}", (b'A' + n as u8) as char);
  }
  for y in 0..SIZE {
    print!("\n\x1b[{}C{}", offset.0, y + 1);
    for x in 0..SIZE {
      print!(
        " {}",
        match grid[y][x] {
          Cell::Ship if player => '#',
          Cell::Miss if !player => '-',
          Cell::Hit => 'X',
          _ => '~',
        }
      );
    }
  }
  println!();
}

/// display the given prompt and read 1 line from the console
fn prompt(p: &str) -> Result<String, Error> {
  print!("{}: ", p);
  io::stdout().flush()?;
  let mut buf = String::new();
  io::stdin().read_line(&mut buf)?;
  Ok(buf.trim().to_string())
}