# Mixed Nash Equilibrium Simulator

An interactive Godot application for exploring simultaneous games and
population dynamics. It allows users to configure player types and payoff
matrices, then visualize a simulation over multiple generations.

This was a graduation project for a faculty program taught in Serbian.
The interface and in-app explanatory text were required to be in Serbian
and have been kept in their original language.

## Requirements

- Godot 4.6.1 **.NET** edition.
- .NET SDK 10.0.101 or a compatible later 10.0 SDK, as specified by
  `global.json`. The project targets .NET 8.

## Running the project

1. Import `project.godot` into the Godot .NET editor.
2. Build the C# project in the editor and run the main scene.
3. Use the Serbian interface to configure player types and payoff matrices,
   then start the simulation.

The `.godot/` directory contains generated editor and build files and is
not included in this repository.

## Current limitations

The code includes pure and 2-by-2 mixed equilibrium calculation helpers,
but the general equilibrium finder is a placeholder and the helpers are
not currently connected to the simulation. Expected mixed payoffs are also
placeholders. Although the setup allows more strategies, the payoff-entry
screen and simulation currently use two strategies per player type.

## Project layout

- `scenes/` contains the Godot scenes and interface.
- `scripts/` contains the C# simulation and interface code.
- `project.godot` and `NashEquilibrium.csproj` contain project settings.
