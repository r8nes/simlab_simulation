# Godland — source showcase

Scripts of **Godland**, a PC roguelike in Unity 6 (URP). The player is a *writer*: they shape a 7×7 world of tiles and nudge a living simulation through epochs, sagas and catastrophes over ~3000 years. The world is never scripted. Settlements, kingdoms, races, gods, pantheons and titans each choose their own actions.

This repository holds code only: no art, configs or third-party plugins. It is meant to be read, not opened as a Unity project.

## Stack

Unity 6000.3 · URP · VContainer (DI) · UniTask · PrimeTween · new Input System · Odin Inspector (configs; not included).

## Assemblies

| Assembly | Role |
|---|---|
| `Core` | Pure simulation, no engine references. Deterministic (`SeededRandom`, stable ordering). |
| `Game` | Content logic on top of Core, ScriptableObject configs, MVP presentation, DI composition. |
| `Editor` | Monte-Carlo batch simulator and SimLab build tooling. |

Both main assemblies are split into the same modules, and namespaces follow folders:

| Module | Contents |
|---|---|
| **World** | Grid, tiles, fertility, climate, biomes, volcanoes, corruption, peaks |
| **Life** | Creatures with inheritable genes, settlements and faiths, cities, kingdoms and dynasties, races, heroes, `Mind` (utility AI) |
| **Powers** | Elemental gods, real pantheons with GOAP schemes, titans, sovereigns, prophecies |
| **Fate** | Epochs, catastrophes, anomalies, sagas, emergence (world memory → echoes), world events |
| **Writer** | Player tools: tile actions, commands, dilemmas, commandments, gifts, progression |
| **Presentation** | Camera, input, journal, chronicle, overview, inspection, menu (MVP views/presenters) |

## Key ideas

- **One world graph.** `Game/Simulation/SimulationInstaller` registers every simulation service. The game (`GameLifetimeScope`), the headless simulator and SimLab all build the same graph.
- **Explicit yearly order.** `WorldSimulation.AdvanceYear()` runs Core steps, then the ordered `IWorldStep` list in `Game/Simulation/WorldStepOrder.cs`. Nothing depends on registration order.
- **No MonoBehaviour lifecycle.** All lifecycle goes through VContainer interfaces, async through UniTask, and UI is passive-view MVP.
- **Deterministic.** The same seed gives an identical 3000-year run, so refactors are checked by diffing Monte-Carlo reports.
- **Every tile change has a cause.** `WorldTiles.Place(cell, id, cause)` feeds tile history, the journal and world memory.

## SimLab

`Game/SimLab` is a WebGL/WebGPU test harness. It shows every config field of the simulation as an editable column and runs N bot-played worlds year by year, with a downloadable log.
