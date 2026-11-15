# Third-party components

This source snapshot does not bundle the .NET runtime or System.IO.Ports binaries.

- .NET 10 and Windows Forms: Microsoft/.NET Foundation components; see https://github.com/dotnet/runtime and https://github.com/dotnet/winforms for component license and notice information.
- System.IO.Ports 10.0.0: NuGet dependency; see https://www.nuget.org/packages/System.IO.Ports/10.0.0 and its linked license/source. Direct and transitive package versions are recorded in packages.lock.json files.
- GitHub Spec Kit: upstream provenance and its included MIT license are documented in docs/SPEC-KIT.md and the Spec Kit notice files created during setup.
- GitHub Actions checkout and setup-dotnet are CI tooling, referenced by immutable commit hashes in the workflow.

A self-contained Windows release bundles runtime components. Review and include the applicable runtime/package license and notice files in each distributable. This inventory is not a legal opinion or a redistribution-rights determination.

## Packaged releases

`build.ps1` copies the original runtime/package license and notice files from the exact NuGet versions referenced by the published dependency manifest into `ThirdPartyNotices/`. System.IO.Ports declares MIT in its preserved NuGet manifest; the .NET Foundation runtime MIT text is included alongside its original third-party notice. The project MIT license does not replace those component terms. No USB/serial drivers or Spec Kit tooling are bundled in the portable application.
