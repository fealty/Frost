# Overview

Frost is a hardware accelerated drawing and composition library written in C#. The API takes inspiration from the HTML 5 Canvas element and integrates powerful drawing, shaping, and composition APIs unified by device-based lazy resource management. The library itself does not rely upon a specific platform, but the reference implementation relies upon Windows and DirectX through SharpDX.

# Use Cases

Frost is written and designed to be fast, so it should be suitable for anything ranging from texture generation in video games to UI rendering.

# Usability

The project is in beta stage without much documentation. The library and reference implementation are practically complete and stable, barring the implementation of signed distance fields.

# How to Use

Simply grab a copy of the repository, open the solution file in VS 2010, and build the project in release. In your own projects, if you use only platform agnostic functionality, just reference `Frost.dll` in your project. If you need platform specific functionality, you'll need to reference all of the implementation libraries as well.