# Interop
Interop exploration in C#

## Summary

This project provides a solution with a native library and a test project and performance console app for exploring interop in C#.

## Usage

Clone and open in VS. Requires VS2019 and .NET Core 3.0. (.NET Core prerelease builds require checking "Use  previews of the .NET Core SDK" in VS Options).

Select the x64 build for the solution. To explore/run the tests show the Test Explorer window and set the processor settings to x64 in the Test menu.

Set the PerfTest project to startup and run after setting release configuration. To run different tests, change the class in the `BenchmarkRunner` in `Program.Main()`.

## Contributions

Contributions are welcome.
