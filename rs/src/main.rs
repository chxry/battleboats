#![feature(generic_arg_infer, slice_flatten)]
use std::{io, mem};
use std::io::{Write, Read};
use std::fs::File;
use nanorand::{Rng, WyRand};

type Error = Box<dyn std::error::Error>;

const SIZE: usize = 8;
const SHIPS: &[(usize, u8)] = &[(2, 1), (2, 2), (1, 3)];
const SAVE_FILE: &str = "game.bin";
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
      "p" | "play" => play(input_ships()?, gen_ships())?,
      "r" | "resume" => {
        let mut f = File::open(SAVE_FILE)?;
        play(load_grid(&mut f)?, load_grid(&mut f)?)?;
      }
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

/// main gameplay loop
fn play(mut player_grid: Grid, mut pc_grid: Grid) -> Result<(), Error> {
  let mut rng = WyRand::new();
  let player_won = loop {
    // save
    let mut f = File::create(SAVE_FILE)?;
    save_grid(&player_grid, &mut f)?;
    save_grid(&pc_grid, &mut f)?;

    display_grid(&player_grid, "fleet grid", (0, 0), true);
    display_grid(&pc_grid, "tracker grid", ((SIZE + 2) * 2, SIZE + 3), false);

    // player turn
    loop {
      match parse_coord(&prompt("enter target coord")?) {
        Ok(c) => match pc_grid[c.1][c.0] {
          Cell::Miss | Cell::Hit => println!("coordinate already picked"),
          Cell::Empty => {
            println!("miss");
            pc_grid[c.1][c.0] = Cell::Miss;
            break;
          }
          Cell::Ship => {
            println!("hit");
            pc_grid[c.1][c.0] = Cell::Hit;
            break;
          }
        },
        Err(e) => println!("{}", e),
      }
    }
    if !pc_grid.flatten().contains(&Cell::Ship) {
      break true;
    }

    // computer turn
    print!("computer's turn: ");
    io::stdout().flush()?;
    loop {
      let x = rng.generate_range(0..SIZE);
      let y = rng.generate_range(0..SIZE);
      // remove duplicate logic here
      match player_grid[y][x] {
        Cell::Empty => {
          println!("{}", fmt_coord(x, y));
          println!("miss");
          player_grid[y][x] = Cell::Miss;
          break;
        }
        Cell::Ship => {
          println!("{}", fmt_coord(x, y));
          println!("hit");
          player_grid[y][x] = Cell::Hit;
          break;
        }
        _ => {}
      }
    }
    if !player_grid.flatten().contains(&Cell::Ship) {
      break false;
    }
  };
  if player_won {
    println!("you won");
  } else {
    println!("you lost");
  }
  Ok(())
}

/// parse the given string (case insensitive) as a coordinate (0-based) (B5 -> (1,4))
/// verifies if the coordinate is within (SIZE, SIZE)
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

/// returns a string representing the given coordinate ((2,3) -> D4)
fn fmt_coord(x: usize, y: usize) -> String {
  format!("{}{}", (b'A' + x as u8) as char, y + 1)
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
          Cell::Ship if player || DBG_SHOW_ENEMY_SHIPS => '#',
          Cell::Miss if !player => '-',
          Cell::Hit => 'X',
          _ => '~',
        }
      );
    }
  }
  println!();
}

fn save_grid<W: Write>(grid: &Grid, w: &mut W) -> Result<(), Error> {
  w.write_all(&grid.iter().flatten().map(|c| *c as u8).collect::<Vec<_>>())?;
  Ok(())
}

fn load_grid<R: Read>(r: &mut R) -> Result<Grid, Error> {
  let mut buf = [0; SIZE * SIZE];
  r.read_exact(&mut buf)?;
  Ok(unsafe { mem::transmute(buf) }) // lazy
}

/// allow the player to pick positions for ships
fn input_ships() -> Result<Grid, Error> {
  println!("place your ships");
  let mut grid = [[Cell::Empty; _]; _];
  for (i, (num, size)) in SHIPS.iter().enumerate() {
    let mut n = 0;
    while n < *num {
      display_grid(&grid, "fleet grid", (0, 0), true);
      println!(
        "placing ship {}/{}, length {}",
        SHIPS.iter().take(i).map(|c| c.0).sum::<usize>() + n + 1,
        SHIPS.iter().map(|c| c.0).sum::<usize>(),
        size
      );
      let vert = loop {
        match &*prompt("enter ship direction (h/v)")?.to_lowercase() {
          "h" => break false,
          "v" => break true,
          _ => println!("unknown direction"),
        }
      };
      match parse_coord(&prompt("enter ship coord")?)
        .and_then(|c| verify_and_place(&mut grid, c, vert, *size))
      {
        Ok(_) => n += 1,
        Err(e) => println!("{}", e),
      }
    }
  }
  Ok(grid)
}

/// generate random ships
fn gen_ships() -> Grid {
  let mut rng = WyRand::new();
  let mut grid = [[Cell::Empty; _]; _];
  for (num, size) in SHIPS {
    let mut n = 0;
    while n < *num {
      if verify_and_place(
        &mut grid,
        (rng.generate_range(0..SIZE), rng.generate_range(0..SIZE)),
        rng.generate(),
        *size,
      )
      .is_ok()
      {
        n += 1;
      }
    }
  }
  grid
}

/// verifies that the ship placement given is valid and places the ship if it is
fn verify_and_place(
  grid: &mut Grid,
  mut pos: (usize, usize),
  vert: bool,
  size: u8,
) -> Result<(), &str> {
  let mut g = *grid;
  for _ in 0..size {
    if !((0..SIZE).contains(&pos.0) && (0..SIZE).contains(&pos.1)) {
      return Err("ship out of bounds");
    }
    if g[pos.1][pos.0] != Cell::Empty {
      return Err("cell already occupied");
    }
    g[pos.1][pos.0] = Cell::Ship;
    if vert {
      pos.1 += 1;
    } else {
      pos.0 += 1;
    }
  }
  *grid = g;
  Ok(())
}

/// display the given prompt and read 1 line from the console
fn prompt(p: &str) -> Result<String, Error> {
  print!("{}: ", p);
  io::stdout().flush()?;
  let mut buf = String::new();
  io::stdin().read_line(&mut buf)?;
  Ok(buf.trim().to_string())
}
