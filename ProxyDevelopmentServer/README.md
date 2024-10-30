# DarkPatterns.ProxyDevelopmentServer

A development server for use with .NET Spa Services. This works very
similarly to the `UseReactDevelopmentServer` middleware provided by the .NET Spa
Services project.

_Note:_ This server is intended for development purposes only; it is not
optimized for production.

Add your project reference to your csproj; be sure to exclude it for release
builds:

```xml
  <ItemGroup>
    <PackageReference Include="DarkPatterns.ProxyDevelopmentServer" Version="0.1.0" Condition=" '$(Configuration)' == 'Debug' " />
  </ItemGroup>
```

Usage:

```csharp
app.UseSpa(spa => {
#if DEBUG
    if (env.IsDevelopment())
    {
        spa.Options.SourcePath = "./path-to-react-source";
        spa.UseProxyDevelopmentServer(
            // The bin path to run to launch the server from the SourcePath.
            // This is typically "node_modules/.bin/vite", but if you wrap Vite
            // a different path may be necessary
            "node_modules/.bin/vite",
            // Command line arguments to the script.
            // `{port}` will be replaced with an unused port.
            "--port {port}"
        );
    }
#endif
});
```

See the demo project for more details.
