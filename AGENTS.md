# AGENTS.md: Larje Character

Instructions for coding agents working on this package.

## Package layout

- `Runtime/`: `Larje.Character`, which contains abilities, damage and health, usable items and weapons, AI brains (actions, decisions, anchor controllers), FPS camera, cursor service and player provider.
- `Zenject/`: `Larje.Character.Zenject`, which contains `LarjeCharacterInstaller` and `BindLarjeCharacter`.
- `Editor/AI/`: MCP tools `Larje.ListAITypes`, `Larje.DescribeAIBrain` and `Larje.EditAIBrain`.

## Notes

- Zenject injects components before their `Awake`. A `Construct` method must not depend on fields initialised in `Awake`. Use lazy `GetComponent`, and share one subscribed flag between `Construct` and `OnEnable`.
- New AI actions and decisions should keep their fields serializable. The MCP tools discover them by reflection.

## Code style (C#)

- Do not use `var`. Write explicit types.
- Do not write comments in code.
- Do not use `continue`. Restructure the loop or extract a method.
- Seal classes when they are not designed for inheritance.
- Use block bodies `{ }` for methods and `if` statements. Expression-bodied members are fine for simple properties when the surrounding code already uses them.
- Do not name variables `value` except in property setters.
- Make private methods `static` only when a static context requires it.
- Order members as: constants, static fields, serialized fields, fields, properties, events, methods, nested enums, nested classes. Within each group order by access: public, protected, internal, private.
- Private non-serialized fields use `_camelCase`, constants use `UPPER_SNAKE_CASE`. `[SerializeField]` fields follow the style already used in the package (mostly `camelCase`); never rename an existing serialized field without `[FormerlySerializedAs]`, because assets and `com.larje.projectsetup` reference fields by name.
- Serialized fields: never use `[Tooltip]`. Group related fields with `[Header]` and avoid single-field headers.

## Architecture rules

- Services are plain C# classes with constructor injection. Do not add a service locator, singletons or a custom DI container.
- Use UniTask with a `CancellationToken` for loops, delays and async work. Do not use `Update` loops or coroutines in services.
- Ids are ScriptableObject assets. Persistent data lives in save sections owned by the feature.
- Zenject code goes only into the `*.Zenject` assembly. It compiles under `LARJE_ZENJECT`, which comes from `versionDefines` on `com.larje.zenject`.
- Unity AI MCP tools go only into the `*.AI.Editor` assembly. It compiles under `LARJE_UNITY_AI`. Tool names use the `Larje.` prefix and the `larje` group. Put logic that tests need into the regular editor assembly so the tests do not depend on Unity AI.
- Runtime assemblies must not reference editor-only APIs outside `#if UNITY_EDITOR`.

## Working in a Unity project

- Do not edit `.prefab`, `.unity`, `.asset` or `.mat` files as text. Change them through the Unity Editor or MCP tools.
- Commit the `.meta` file for every new file and folder. Git packages are immutable, so missing metas break consumers.
- After every C# change, refresh the editor, wait until compilation is idle, and check that the console has no errors or warnings before moving on.
- Run EditMode tests through the Test Runner. List the package in the project manifest `testables` first.

## Commits

- Write short technical commit messages only. Do not push unless explicitly asked.
