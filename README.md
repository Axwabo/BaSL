# BaSL

This project is a not-very/very-not accurate version of the bash shell in .NET.

The project simulates file systems and executables without restricting the developer too much.
Library authors still have access to all of the BCL (Base Class Library).

# Console Version

You can run the `BaSL.Shell.Console` executable in your terminal.

The experience is comparable to a Linux terminal, albeit limited (see features below).

# Standard Pipes

- `stdin` (standard input) is the default pipe programs read from, e.g. the console
- `stdout` (standard output) is the default pipe to which programs write results or feedback to
- `stderr` (standard error) is the default pipe that error messages are written to

# Features

BaSL is currently capable of piping `|` and standard output redirection `>` and `>>`

Piping `|` means "send stdout to the stdin of another process"

`>` sends stdout to the file, truncating the file if it exists.

`>>` also redirects stdout to a file, but it appends the file instead of truncating it.

## Built-In Programs

- `bytes` prints the first 32 bytes of a file to stdout
- `cat` prints a file's contents to stdout if specified, otherwise it mirrors stdin to stdout
- `cd` changes the current directory of the active shell
- `chmod` changes the mode of the specified files (recursive `-r`)
- `echo` prints the arguments separated by a space to stdout, and writes a newline at the end
- `ls` lists a directory's contents or existing files if a file is specified
- `mkdir` creates a directory
- `pwd` prints the current directory
- `rm` removes file(s), or directories if the recursive flag is specified (`-r`)
- `rmdir` removes an empty directory
- `sleep` waits for the specified amount of seconds (it's very inaccurate for some reason)
- `touch` creates an empty file
- `whoami` prints the current user's username