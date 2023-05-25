This extends the "02_BasicAnnotations" example by adding the `JXS.Ecs.Generators` dependency to generate
fast and convenient code for our systems.

NOTE: If you add `Ecs.Generators` as a local (non-NuGet) dependency, make sure it is listed as
`OutputItemType="Analyzer" ReferenceOutputAssembly="false"`, otherwise it will not work.