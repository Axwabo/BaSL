# BaSL

This project is a not-very/very-not accurate, high-level recreation of the bash shell in .NET.

The project simulates file systems and executables without restricting the developer too much.
Library authors still have access to all of the BCL (Base Class Library).

> [!IMPORTANT]
> This project is not meant for production use!

> [!TIP]
> To integrate `BaSL` as a library, see [this section](#library)

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

# Features

## Standard Pipes

- `stdin` (standard input) is the default pipe programs read from, e.g. the console
- `stdout` (standard output) is the default pipe to which programs write results or feedback to
- `stderr` (standard error) is the default pipe that error messages are written to

BaSL is currently capable of piping `|` and standard output redirection (`>` and `>>`) with some limitations.
Standard stream selection is not supported yet.

Piping `|` means "send stdout to the stdin of another process"

`>` sends stdout to the file, truncating the file if it exists.

`>>` also redirects stdout to a file, but it appends the file instead of truncating it.

`<` sets the stdin of the process specified in the left operand to the file in the right operand.

## Syntax

BaSL currently features simple variable expansion, quoted and "verbatim" strings.

### Variables

The syntax to define variables is the following: `variable=value`

If you specify a command after the variable declaration(s), the variables will only be set for that statement.

Use `$sus` to expand variable named "sus"

Variable expansion is not performed in [verbatim strings](#verbatim-string)

If the variable is in "quotes," word splitting will not be performed.

Example:

```bash
AMONG=sus
echo $HOME $AMONG
AMONG=er bash -c 'echo $AMONG'
echo $AMONG
```

Result:

```
/home/user sus
er
sus
```

> [!NOTE]
> Command-scoped variable definitions are only possible at the beginning of a statement
> (not per command in a pipeline) for now.

### Verbatim Strings

Use `'text'` to pass `text` that is interpreted literally.
Variable expansion and escaping are not performed in these strings.

Example:

```bash
echo 'among$us\in real life'
```

Result: `among$us\in real life`

### Quoted Strings

Quoted strings allow variable expansion while allowing word separators to be passed in literally.

Word splitting is not performed when expanding variables.

```bash
# word separator
IFS=m
us="among us"
echo $us "among$us\in real life"
```

Result: `a ong us amongamong us\nin real life`

Notice that the unquoted variable was split at `m` while the quoted variable was left intact. 

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
- "Unlimited" pipelines
- `for` and `while` loops
- `case` statements
- Proper input handling
- A lot more unknowns

## Shell Built-Ins

These commands are always available:

- `help` lists available commands; use `help <command>` to get help for a specific one
- `let` sets a variable to the result of a simple arithmentic expression (addition, subtraction, multiplication, division, modulo)
- `unset` removes a variable from the local and exported dictionary
- `export` makes a variable available to subshells

## Built-In Programs (CoreUtils)

As a developer, you'll need to call `operatingSystem.InstallCoreUtilsAsync()` to add these programs:

- `bytes` prints the first 32 bytes of a file to stdout
- `cat` prints a file's contents to stdout if specified, otherwise it mirrors stdin to stdout
- `cd` changes the current directory of the active shell
- `chmod` changes the mode of the specified files (recursive `-r`)
- `clear` clears `System.Console` (`Terminal` project only)
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