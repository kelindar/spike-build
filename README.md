Spike.Build
===========

![Spike Engine](https://s3.amazonaws.com/cdn.misakai.com/www-spike/logo@2x.png)

Spike.Build is the set of libraries that perform parsing and code-generation for creating various client SDKs for [Spike Engine](http://www.spike-engine.com).


* **Spike.Build.Runtime** - The main runtime that performs .spml definition parsing and contains server-side code generation for the particular model.
* **Spike.Build** - A simple command-line tool that can be used for client SDK generation.
* **Spike.Build.AS3** - ActionScript 3 Client SDK generator.
* **Spike.Build.CSharp** - C# Client SDK generator.
* **Spike.Build.JavaScript** - JavaScript Client SDK generator.


Spike.Build - NEXTGEN
=====================

The [nextgen branch](https://github.com/Kelindar/spike-build/tree/nextgen) currently contains most recent version of Spike.Build. We are in the process of porting our master branch to the nextgen branch.  
* Build status: [![Build status](https://ci.appveyor.com/api/projects/status/pj2081d7m07mu54d?svg=true)](https://ci.appveyor.com/project/Kelindar/spike-build)
* Current Binaries: [spike-build.zip](http://pub.misakai.com/bin/spike-build.zip)

Currently supported platforms on the [nextgen branch](https://github.com/Kelindar/spike-build/tree/nextgen):
 * .NET / C#5 with async/await support for non-blocking networking
 * Xamarin for .NET on Android & iOS
 * Java and Java Android
 * WinRT that supports Windows 8+ and Windows Phone 8+
