# Axiom Verge Mod Tools

_Note: This README is a work in progress. I'm prioritizing releasing the project, and will work on the README incrementally._

## About

AVMT is a patcher for installing the [Axiom Verge Mod Loader](https://github.com/saberamesia/AVModLoader) as well as installing hooks to insert new behavior into the game.
It requires the `AVModLoader.dll` from the mod loader project to function correct.

## Dependencies

This project requires references to the Axiom Verge game executable and to FNA to build. It also requires:
- Mono.Cecil for patching in hooks
- ILRepack.lib for patching in the mod loader library

## Instructions (Windows)

Place `AVModTools.exe` and the required DLLs, including `AVModLoader.DLL`, in your Axiom Verge directory. Run `AVModTools` to patch the mod loader into the game.

## Future Plans

- Install additional hooks
- Possible path for overriding existing game behaviors entirely