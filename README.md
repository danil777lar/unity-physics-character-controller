# Larje Character

`com.larje.character`: a physics-based character controller with abilities, health and damage, usable items and weapons, state-machine AI brains and an FPS camera.

## Install

UPM cannot resolve git dependencies, so add UniTask, core and this package to `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.character": "https://github.com/danil777lar/unity-physics-character-controller.git"
```

Requires Input System, Cinemachine 3, Physics and AI (NavMesh) modules. Addressables are optional (`LARJE_ADDRESSABLES`).

## Contents

- **Abilities**: `CharacterWalk`, `CharacterRun`, `CharacterJump`, `CharacterCrouch`, `CharacterDash`, `CharacterWallRun`, `CharacterLedgeClimb`, `CharacterKnockback`, `CharacterRagdoller`, `CharacterOrientation`, `CharacterItemUser`.
- **Damage**: health, damage sources, damage types and damage events.
- **Usable items**: a weapon base with projectiles.
- **AI**: brains built from states, actions and decisions, plus anchor controllers for position and look-at.
- **FPS camera**: player camera, camera shaker, and `ICursorService` (`CursorService`) for cursor lock and visibility.
- **Player provider**: `IPlayerProviderService` exposes the current player character.

## With Zenject

Create `Larje/Character/Zenject/Character Installer` (`LarjeCharacterInstaller`), assign a `CursorConfig` and add it to `ProjectContext`. Or call `Container.BindLarjeCharacter(cursorConfig)` from your installer.

Zenject injects scene components before `Awake`, so `Construct` methods do not depend on `Awake` state.

## Unity CLI commands

With `com.unity.pipeline` in the project, a running editor exposes these commands to the [Unity CLI](https://docs.unity.com/en-us/unity-cli) (`unity command <group> <name>`):

| Command | Purpose |
|---|---|
| `larje.ai list_types` | List every AI action and decision type with its serialized fields |
| `larje.ai describe --prefab <asset>` | Describe an AI brain prefab as JSON: states, actions, transitions |
| `larje.ai edit --prefab <asset> (--ops '<json array>' \| --file <ops.json>)` | Apply a list of edit operations to an AI brain prefab, saved once |
