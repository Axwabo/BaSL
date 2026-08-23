# BaSL.SourceGenerators

This project generates members to make `App` writing easier.

The following generators are available:

- [ConstructorGenerator](#constructor)
- [HelpGenerator](#help)
- [ExecuteGenerator](#execute)

> [!IMPORTANT]
> You will need to mark your custom `App` class `partial` for source generators to work.

# Installation

Since the package isn't published to a NuGet host, installation requires a local source.

1. Create a directory to store the NuGet package in
2. Download the `BaSL.*.*.*.nupkg` file from the [releases page](https://github.com/Axwabo/BaSL/releases)
3. Place the NuGet package in your chosen directory
4. Copy the **fully qualified path** of the directory
5. Run `dotnet nuget add source "$DIR" --name BaSL` in your terminal
    - Replace `$DIR` with the fully qualified path
6. Reference the package
    - Add the following into an `ItemGroup` in your .csproj: `<PackageReference Include="BaSL.SourceGenerators" Version="*.*.*" PrivateAssets="all" />`
    - Replace `*.*.*` with the version you downloaded

# Available Generators

## Constructor

A constructor is generated if no constructor is declared in a class that **directly** inherits from `BaSL.Executables.App`

This eliminates the following boilerplate code:

```csharp
public MyApp(ExecutableContext context) : base(context)
{
}
```

## Help

Add the `BaSL.Executables.HelpAttribute` to a class instead of implementing `IHelpProvider`

<details>
<summary>Example</summary>

```csharp
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

[Help("This will be displayed when the executable is in the PATH, and the user invokes help with its name.")]
public partial sealed class MyApp : App
{

    public override async Task<int> ExecuteAsync(CancellationToken token)
    {
        // app implementation
    }

}
```

</details>

## Execute

Arguments can be automatically parsed into method parameters by marking a method with the `BaSL.Executables.Attributes.ExecuteAttribute`

<details>
<summary>Example</summary>

```csharp
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using Directory = BaSL.FileSystems.Directory;

public partial sealed class MyApp : App
{

    // usages:
    // myapp 1
    // myapp -s 1 /home
    // myapp 2 /home sus mogus
    // myapp 2 /home -- sus mogus
    [Execute]
    public async Task<int> MogusAsync(
        int? amount,
        [DefaultTo(DefaultDirectory.UserHome)] Directory directory,
        Args rest,
        [Flag] bool sus = false,
        CancellationToken token = default
    )
    {
        // app implementation
    }

}
```

</details>

> [!NOTE]
> Parameter parsing is limited for now. Value type parameters (except for flags) must be declared nullable.

### Parameters

If `--` is an argument, parsing is stopped and, all arguments after it are passed as the `rest` argument (if any).
The rest argument must be of type `BaSL.Args`

The cancellation token is passed to parameters of type `System.Threading.CancellationToken`

The `FlagAttribute` marks a bool that is true if any arg starting with `-` contains it.

Parameters that don't match any of the above will be parsed as positional arguments.

Arguments of type `BaSL.FileSystems.Directory` resolve a directory, and quit if the directory was not found.
If the argument is not specified, the default value can be specified by adding the `DefaultToAttribute`

Other arguments are parsed using `BaSL.Executables.ArgumentParser` that quit if parsing is unsuccessful.
Strings are passed as-is.
Built-in parsers exist for the following types: `bool` `float` `double` `int` `byte`