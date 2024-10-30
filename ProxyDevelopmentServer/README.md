# DarkPatterns.ProxyDevelopmentServer

A development server for use with .NET Spa Services. This works very similarly
to the `UseReactDevelopmentServer` middleware provided by the .NET Spa Services
project.

_Note:_ This server is intended for development purposes only; it is not
optimized for production.

Add your project reference to your csproj; be sure to exclude it for release
builds:

```xml
  <ItemGroup>
    <PackageReference Include="DarkPatterns.ProxyDevelopmentServer" Version="0.1.0" Condition=" '$(Configuration)' == 'Debug' " />
  </ItemGroup>
```

Usage example:

```csharp
app.UseSpa(spa => {
#if DEBUG
    if (env.IsDevelopment())
    {
        spa.Options.SourcePath = "./path-to-working-directory";

        spa.UseProxyDevelopmentServer(DarkPatterns.ProxyDevelopmentServer.ProxyDevelopmentServerOptions.PnpmViteServer);
        // or
        spa.UseProxyDevelopmentServer(DarkPatterns.ProxyDevelopmentServer.ProxyDevelopmentServerOptions.DefaultViteServer);
        // or
        spa.UseProxyDevelopmentServer(new()
        {
          // fill in your own details
        });
    }
#endif
});
```

The fully-qualified namespace is provided so that the `#if DEBUG` does not need
to be added in the using statements.

- `spa.Options.SourcePath` is the working directory (relative to your
  application's working directory) in which to run the proxy server.
- `ProxyDevelopmentServerOptions` has a few default configurations:
    - `ProxyDevelopmentServerOptions.PnpmViteServer` runs a Vite dev server via
      the PNPM command line.
    - `ProxyDevelopmentServerOptions.DefaultViteServer` runs a Vite dev server
      via the `node_modules/.bin/vite` path.

    If you have a setup that doesn't use one of the standard scenarios, you can
    set up your own configuration:
    - `IsScriptFile` indicates if the file being targeted is a script. If false,
      it is assumed that the command is on the path and does not need additional
      modification (on non-Windows machines, see below.)
    - `BaseCommand` is the name of the executable or script to launch, such as
      `pnpm` or `node_modules/.bin/vite`.
    - `Parameters` are the command-line arguments to be passed to the script.
      These should be already-escaped values and quoted as necessary.
    - `ReadyText` is the text to search for within the Standard Out of the
      command to know that the proxy is ready. This is parsed as a regular
      expression, and evaluated against each complete line returned from the
      command (prior to any filters).
    - `Timeout` is how long to expect to wait for the `ReadyText`. If the
      timeout is exceeded, the application will not start. Note that this is
      used in conjunction with the `spaBuilder.Options.StartupTimeout`; if
      either times out before the proxy is ready, the application will not
      start.

See the demo and test projects for more usage details.
