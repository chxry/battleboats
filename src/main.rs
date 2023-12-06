use std::io;
use std::io::Write;

const GRID_SIZE: u8 = 8;

/// show the menu to the user until q/quit is entered
fn main() -> Result<(), io::Error> {
  loop {
    println!("- Battleboats -");
    println!("p - play a new game");
    println!("r - resume a game");
    println!("i - read instructions");
    println!("q - quit");
    match &*prompt("enter menu option")?.to_lowercase() {
      "p" | "play" => loop {
        println!("{:?}", parse_coord(&prompt("test")?))
      },
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

/// parse the given string as a coordinate (B5 -> (2,5))
/// verifies if the coordinate is within the GRID_SIZE
fn parse_coord(c: &str) -> Result<(usize, usize), &str> {
  let c = c.chars().collect::<Vec<_>>();
  if c.len() == 2 {
    Err(".")
  } else {
    Err("expected 2 characters")
  }
}

/// display the given prompt and read 1 line of console input
fn prompt(p: &str) -> Result<String, io::Error> {
  print!("{}: ", p);
  io::stdout().flush()?;
  let mut buf = String::new();
  io::stdin().read_line(&mut buf)?;
  Ok(buf.trim().to_string())
}
