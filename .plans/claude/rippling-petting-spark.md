# Plan: Update Solution and References for Moved AiDevs4Aspire

## Context

The user moved the `AiDevs4Aspire` project from root-level `AiDevs4Aspire/` to `AiDevs4/aspire/`. The solution file and project references need to be updated to reflect this new location.

## Changes Required

### 1. Add AiDevs4Aspire to Solution File

**File:** `AiDevs.slnx`

Add a new project entry for `AiDevs4\aspire\AiDevs4Aspire.csproj`:

```xml
<Project Path="AiDevs4\aspire\AiDevs4Aspire.csproj" Type="Classic C#">
  <Configuration Solution="Debug|x64" Project="Debug|Any CPU" />
  <Configuration Solution="Debug|x86" Project="Debug|Any CPU" />
  <Configuration Solution="Release|x64" Project="Release|Any CPU" />
  <Configuration Solution="Release|x86" Project="Release|Any CPU" />
</Project>
```

### 2. Update Project Reference in AiDevs4Aspire.csproj

**File:** `AiDevs4/aspire/AiDevs4Aspire.csproj`

Change the ProjectReference from:
```xml
<ProjectReference Include="..\AiDevs4\src\AiDevs4.csproj" />
```

To:
```xml
<ProjectReference Include="..\src\AiDevs4.csproj" />
```

(Since `AiDevs4Aspire.csproj` is now at `AiDevs4/aspire/`, the relative path to `AiDevs4/src/` is `../src/`)

## Verification

```bash
# Build the solution to verify references
dotnet build AiDevs.slnx

# Or build just the Aspire project
dotnet build AiDevs4/aspire/AiDevs4Aspire.csproj
```
