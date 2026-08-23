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

    public async Task<int> ExecuteAsync(CancellationToken token)
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

    public async Task<int> MogusAsync(
        [DefaultTo(DefaultDirectory.UserHome)] Directory directory,
        int? amount,
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

### Parameter Attributes

