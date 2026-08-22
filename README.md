# BaSL

This project is a not-very/very-not accurate, high-level recreation of the bash shell in .NET.

The project simulates file systems and executables without restricting the developer too much.
Library authors still have access to all of the BCL (Base Class Library).

> [!IMPORTANT]
> This project is not meant for production use!

# Console Version

You can run the `BaSL.Terminal` executable in your terminal.

The experience is comparable to a Linux terminal, albeit limited (see features below).

## Setup 

1. Download the appropriate archive from the [releases page](https://github.com/Axwabo/BaSL/releases)
    - Linux: `BaSL.Terminal-linux-x64.zip`
    - windows: `BaSL.Terminal-lose-x64.zip`
2. Extract the archive to a folder of your choosing
3. Run `BaSL.Terminal` from your terminal
    - `BaSL.Terminal.exe` on windows

## Mounts

Directly mounting "real" devices to a BaSL filesystem is not supported (yet).

You can specify "physical" directories to be copied into virtual filesystems in `/media`
by passing arguments to the program where the mounted folder's name and the directory location are separated by `=`

Example: `./BaSL.Terminal amogus=~/Documents sus=/home/user/Desktop`

# Standard Pipes

- `stdin` (standard input) is the default pipe programs read from, e.g. the console
- `stdout` (standard output) is the default pipe to which programs write results or feedback to
- `stderr` (standard error) is the default pipe that error messages are written to

# Features

BaSL is currently capable of piping `|` and standard output redirection (`>` and `>>`) with some limitations.
Standard stream selection is not supported yet.

Piping `|` means "send stdout to the stdin of another process"

`>` sends stdout to the file, truncating the file if it exists.

`>>` also redirects stdout to a file, but it appends the file instead of truncating it.

`<` sets the stdin of the process specified in the left operand to the file in the right operand.

## Limitations

For now, pattern matching is used to parse and execute statements:

- `command [args] < source` is supported, but no additional redirections can be made (no piping or file redirection)
    - `command [args] < source > sink` is parsed but not executed 
    - piping is not supported (e.g. `command [args] < source | other [args]`)
- `command [args] > sink` is supported, but no additional redirections can be made (no piping or file redirection)
- `command [args] | other [args]` is supported with an arbitrary amount of pipes
    - file redirection is not supported

Other features that are yet to be implemented:

- `elif`
- More conditional operators and command support for if statements
- Arrays
- Proper variable expansion and separation
- `$IFS`
- `for` and `while` loops
- `case` statements
- Proper input handling
- A lot more unknowns

## Built-In Programs

- `bytes` prints the first 32 bytes of a file to stdout
- `cat` prints a file's contents to stdout if specified, otherwise it mirrors stdin to stdout
- `cd` changes the current directory of the active shell
- `chmod` changes the mode of the specified files (recursive `-r`)
- `echo` prints the arguments separated by a space to stdout, and writes a newline at the end
- `env` prints the user's environment variables
- `ls` lists a directory's contents or existing files if a file is specified
- `mkdir` creates a directory
- `pwd` prints the current directory
- `rm` removes file(s), or directories if the recursive flag is specified (`-r`)
- `rmdir` removes an empty directory
- `sleep` waits for the specified amount of seconds (it's very inaccurate for some reason)
- `sudo` runs the arguments as a command with superuser privileges
- `touch` creates an empty file
- `whoami` prints the current user's username