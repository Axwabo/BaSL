# BaSL

This project is a not-very/very-not accurate, high-level recreation of the bash shell in .NET.

The project simulates file systems and executables without restricting the developer too much.
Library authors still have full access to the BCL (Base Class Library).

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

BaSL currently features basic branching, simple variable expansion, quoted and "verbatim" strings.

### Variables

The syntax to define variables is the following: `variable=value`

If you specify a command after the variable declaration(s), the variables will only be set for that statement.

Use `$sus` to expand variable named "sus"

Variable expansion is not performed in [verbatim strings](#verbatim-strings)

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

### Branching

`if-then-else-fi` are supported.

If statements' conditions can be commands (0 exit code = true, other exit code = false), or [double-bracket conditions](#conditions)

The condition must be followed by `;` and the `then` keyword.

The if statement is terminated using the keyword `fi`

<details>
<summary>Example</summary>

```bash
echo "So wake me up when it's all over"

if [[ "$USER" == "root" ]]; then
    echo Hai superuser :3 
else
    echo Who are you???
fi

echo Wakey wakey
```

</details>

### Conditions

Boolean conditions are limited, with some operator precedence.
Parentheses `()` are not supported yet.

Logical operators:

- `&&` or `-a` is logical and (stop and return false if any condition fails)
- `||` or `-o` is logical or (stop and return true if any condition succeeds)
- `&&` takes precedence over `||`
- Arguments between the aforementioned operators are considered conditions

You can prefix a condition with `! ` (space required) to invert the result.

Supported condition operators:

- `true` or `1`
- `false` or `0`
- `x == y` or `x = y` or `x -eq y` checks if x and y are equal
- `x != y` or `x -ne y` checks if x and y are **not** equal
- `x < y` or `x -lt y`
- `x > y` or `x -gt y`
- `x <= y` or `x -le y`
- `x >= y` or `x -ge y`
- `-z str` checks if a string is null or empty (true if length = 0)
- `-n str` checks if a string is not null and not empty (true if length != 0)
- `-e entry` checks if `entry` exists on the file system
- `-f entry` checks if `entry` is a file
- `-d entry` checks if `entry` is a directory
- `-h entry` or `-L entry` checks if `entry` is a symbolic link
- Unsupported conditions evaluate to `false`

> [!IMPORTANT]
> Conditions evaluate to true if the number is 1.
> Commands evaluate to true if the process returns exit code 0.

## Limitations

For now, pattern matching is used to parse and execute statements:

- `command [args] < source` is supported, but no additional redirections can be made (no piping or file redirection)
    - `command [args] < source > sink` is parsed but not executed 
    - piping is not supported (e.g. `command [args] < source | other [args]`)
- `command [args] > sink` is supported, but no additional redirections can be made (no piping or file redirection)
- `command [args] | other [args]` is supported with an arbitrary amount of pipes
    - file redirection is not supported

Other features that are yet to be implemented:

- `elif` (I hate this keyword so much)
- Physical mounts (scary)
- Virtual file system quotas (maybe configurable)
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

# Library

Create a `BaSL.OperatingSystem`, add user(s), then create a `BaSL.Console` for interactions.
You can write to the console's `StandardInput` writer.

> [!IMPORTANT]
> Exposing the `OperatingSystem` instance allows for any library to run commands as the virtual root user.

You can access the file system through the OS's `FileSystem` property.

Apps have access to the current shell, file system, but not to the `OperatingSystem`

The `UserContext` needs to be passed to methods interacting with the file system.

> [!TIP]
> See the [source generator project](Generators/BaSL.SourceGenerators) to assist with creating `App`s.

## Installation

Since the package isn't published to a NuGet host, installation requires a local source.

For development:

1. Create a directory to store the NuGet package in
2. Download the `BaSL.*.*.*.nupkg` file from the [releases page](https://github.com/Axwabo/BaSL/releases)
3. Place the NuGet package in your chosen directory
4. Copy the **fully qualified path** of the directory
5. Run `dotnet nuget add source "$DIR" --name BaSL` in your terminal
    - Replace `$DIR` with the fully qualified path
6. Reference the package
    - Add the following into an `ItemGroup` in your .csproj: `<PackageReference Include="BaSL.SourceGenerators" Version="*.*.*" PrivateAssets="all" />`
    - Replace `*.*.*` with the version you downloaded

As a dependency:

1. Download the `BaSL.dll` file from the [releases page](https://github.com/Axwabo/BaSL/releases)
2. Place the DLL into the adequate directory
