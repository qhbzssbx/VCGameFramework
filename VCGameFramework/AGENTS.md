# Repository Guidelines

## Principles
- **Always reply in Chinese.** Critically and objectively review questions, ideas, approaches, and code. Evaluate performance, scalability, maintainability, readability for future contributors, and robustness. Consider domain/context appropriateness. Avoid bias or validating proposals; provide objective, context-based insights. Reflect before answering.
- **Leave a few methods unimplemented when appropriate.** Provide full design rationale and comments; let users implement to strengthen coding skills and thinking.

## Project Structure & Module Organization
- Root: Unity project with `Assets/`, `Packages/`, `ProjectSettings/`, `UserSettings/`.
- Code: hot-update gameplay logic lives in `Assets/Game/` (HybridCLR hot update layer).
- Non-hotfix (AOT/bootstrap): keep non-hotfix code in a separate AOT assembly or as precompiled DLLs (e.g., `Assets/AssetRaw/DLL/`) referenced by the hot-update layer.
- Scenes & Resources: `Assets/Scenes/`, `Assets/Resources/`, `Assets/Settings/`, `Assets/StreamingAssets/` (includes `yoo/` data for YooAsset).
- Generated/output: `Library/`, `Temp/`, `Logs/`, `obj/` — do not commit.

## Build, Test, and Development Commands
- Open in Editor using the Unity version in `ProjectSettings/ProjectVersion.txt`.
- CLI tests (EditMode): `"<UnityPath>\\Unity.exe" -batchmode -projectPath "<repo>" -runTests -testPlatform EditMode -logFile Logs/tests-edit.log -testResults Logs/editmode-results.xml -quit`
- CLI tests (PlayMode): same as above with `-testPlatform PlayMode`.
- Batch build: trigger your CI entry (e.g., `-executeMethod BuildScripts.CI.Build -buildTarget StandaloneWindows64`) and write logs to `Logs/`.

## Coding Style & Naming Conventions
- Language: C# (Unity). Follow `.editorconfig`.
- Indentation: 4 spaces; UTF-8; one class per file when reasonable.
- Naming: `PascalCase` for public APIs; `camelCase` for locals/params; `_camelCase` for private fields. Use namespaces starting with `VCGameFramework.*`.
- Folder intent: hot-update code under `Assets/Game/`; non-hotfix entry/API in an AOT/DLL assembly; editor scripts under `Assets/Game/Editor/`.

## Testing Guidelines
- Framework: Unity Test Framework (NUnit).
- Layout: `Assets/Tests/EditMode/` and `Assets/Tests/PlayMode/`.
- Naming: files `*Tests.cs`; methods `[Test]`/`[UnityTest]` with clear Arrange/Act/Assert.
- Run: via the Test Runner or the CLI commands above; include minimal repro cases in PRs.

## Commit & Pull Request Guidelines
- Commits: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`). Keep each commit focused.
- Branches: `feature/<short-name>`, `fix/<issue-id>`, `chore/<task>`.
- PRs: provide a summary, linked issues, steps to validate, and the Unity editor version; attach screenshots/GIFs for UI/scene changes. Exclude `Library/` and `Temp/` from diffs.
- Packages: when `Packages/manifest.json` changes, commit `Packages/packages-lock.json` together.

## Security & Configuration Tips
- Pin the project Unity version to avoid reserialization churn.
- Use Git LFS for large binaries and follow `.gitattributes`.
- Keep secrets out of the repo; prefer environment-specific config or placeholders under `StreamingAssets`.
