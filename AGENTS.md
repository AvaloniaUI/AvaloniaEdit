# AGENTS.md

- Run `git submodule update --init` first. `build-common/` provides the shared MSBuild targets
  and NUKE helpers; without it the build fails with a confusing missing-import error.
- Trim/AOT (IL2xxx/IL3xxx) warnings are errors only in Release builds.
  Build with `-c Release` before calling a change done.
- A library under `src/` is packaged only if its csproj imports
  `$(RepositoryTargetsRoot)/PackEnable.targets`.
