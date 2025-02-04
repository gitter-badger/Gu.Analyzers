# Gu.Analyzers

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Gu.Analyzers.svg)](https://www.nuget.org/packages/Gu.Analyzers/)
[![Build status](https://ci.appveyor.com/api/projects/status/wep65e928gyytgvv/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/gu-analyzers/branch/master)

<!-- start generated table -->
<table>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0001.md">GU0001</a></td>
  <td>Name the arguments.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0002.md">GU0002</a></td>
  <td>The position of a named argument should match.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0003.md">GU0003</a></td>
  <td>Name the parameters to match the members.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0004.md">GU0004</a></td>
  <td>Assign all readonly members.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0005.md">GU0005</a></td>
  <td>Use correct argument positions.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0006.md">GU0006</a></td>
  <td>Use nameof.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0007.md">GU0007</a></td>
  <td>Prefer injecting.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0010.md">GU0010</a></td>
  <td>Assigning same value.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0020.md">GU0020</a></td>
  <td>Sort properties.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0021.md">GU0021</a></td>
  <td>Calculated property allocates reference type.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0030.md">GU0030</a></td>
  <td>Use using.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0031.md">GU0031</a></td>
  <td>Dispose member.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0033.md">GU0033</a></td>
  <td>Don't ignore returnvalue of type IDisposable.</td>
</tr>
<tr>
  <td><a href="https://github.com/JohanLarsson/Gu.Analyzers/blob/master/documentation/GU0034.md">GU0034</a></td>
  <td>Returntype should indicate that the value should be disposed.</td>
</tr>
<table>
<!-- end generated table -->

## Using Gu.Analyzers

The preferable way to use the analyzers is to add the nuget package [Gu.Analyzers](https://www.nuget.org/packages/Gu.Analyzers/)
to the project(s).

The severity of individual rules may be configured using [rule set files](https://msdn.microsoft.com/en-us/library/dd264996.aspx)
in Visual Studio 2015.

## Installation

Gu.Analyzers can be installed using [Paket](https://fsprojects.github.io/Paket/) or the NuGet command line or the NuGet Package Manager in Visual Studio 2015.


**Install using the command line:**
```bash
Install-Package Gu.Analyzers
```

## Updating

The ruleset editor does not handle changes IDs well, if things get out of sync you can try:

1) Close visual studio.
2) Edit the ProjectName.rulset file and remove the Gu.Analyzers element.
3) Start visual studio and add back the desired configuration.

Above is not ideal, sorry about this. Not sure this is our bug.


## Current status

Early alpha, names and IDs may change.
