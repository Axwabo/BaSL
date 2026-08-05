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