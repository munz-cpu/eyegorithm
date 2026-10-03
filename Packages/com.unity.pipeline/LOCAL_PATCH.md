# Local Unity Pipeline patch

This embedded copy of `com.unity.pipeline` 0.7.0-exp.1 replaces calls to
`Assembly.GetName().Name` with `PipelineUtils.GetAssemblySimpleName`.

On this project's non-ASCII Windows path, Mono's `Assembly.GetName()` tries to
convert the assembly `CodeBase` and throws `ExecutionEngineException: String
conversion error`. The helper reads `Assembly.FullName`, which comes from
assembly metadata and does not convert the file path. Keep this change when
updating the package until the upstream package handles the same case.

The Editor also uses `Assembly.Load` for in-memory eval assemblies. Unity's
`CurrentAssemblies.LoadFromBytes` calls `Assembly.GetName()` on earlier eval
assemblies and fails on the second evaluation under the same path condition.
