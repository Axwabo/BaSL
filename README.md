# BaSL

This project is a not-very/very-not accurate version of the bash shell in .NET.

The project simulates file systems and executables without restricting the developer too much.
Library authors still have access to all of the BCL (Base Class Library).

# Console Version

You can run the `BaSL.Shell.Console` executable in your terminal.

The experience is comparable to a Linux terminal, albeit limited (see features below).

# Features

BaSL is currently capable of piping `|` and standard output redirection `>` and `>>`

Piping `|` means "send the standard output to the standard input of another process"

`>` sends the standard output to the file, truncating it if the file exists.

`>>` also redirects the standard output to a file, but it appends the file instead of truncating it.

## Built-In Programs

- `cd` changes the current directory of the active shell
- `rmdir` removes an empty directory
- `rm` removes file(s), or directories if the recursive flag is specified (`-r`)
- `cat` prints a file's contents to stdout if specified, otherwise it mirrors stdin to stdout
- `ls` lists a directory's contents or existing files if a file is specified
- `chmod` changes the mode of the specified files (recursive `-r`)
- `mkdir` creates a directory
- `touch` creates an empty file
- `sleep` waits for the specified amount of seconds
- `whoami` prints the current user's username
- `pwd` prints the current directory
- `bytes` prints the first 32 bytes of a file to stdout
- `echo` prints the arguments separated by a space to stdout, and writes a newline at the end