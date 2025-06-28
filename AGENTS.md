---
name: "VelorenPort"
description: "Port of the original Veloren Rust game to C# with a Unity-based client. This directory hosts early planning and modules gradually rewritten for the .NET ecosystem while retaining game logic and assets from the Rust version."
category: "Game Development"
author: "Veloren Community"
authorUrl: "https://gitlab.com/veloren/veloren"
tags: ["game", "port", "unity", "csharp", "rust"]
lastUpdated: "2025-06-28"
---

# VelorenPort

## Project Overview

VelorenPort aims to replicate the Rust project [Veloren](https://gitlab.com/veloren/veloren) using C#. Server and shared logic are rewritten for .NET, while the client is designed to run on the Unity engine. The goal is to maintain feature parity with the original game and provide an accessible development environment for contributors familiar with C# and Unity.

## Tech Stack

- **Frontend**: Unity (C#)
- **Backend**: C#/.NET, referencing original Rust logic
- **Database**: None yet (planned for file-based or lightweight DB storage)
- **Deployment**: Cross-platform builds with .NET and Unity
- **Other Tools**: Git for version control, Cargo/Rust as reference

## Project Structure

```
VelorenPort/
├── CoreEngine/
├── Network/
├── World/
├── Server/
├── Client/       # Unity project
├── Plugin/
├── Simulation/
└── ...
```

## Development Guidelines

### Code Style

- Use a consistent C# formatter (e.g., dotnet-format or editorconfig)
- Follow idiomatic C# and Unity best practices
- Keep code readable and document nontrivial logic

### Naming Conventions

- File naming: `PascalCase.cs`
- Variable naming: `camelCase`
- Function naming: `PascalCase`
- Class naming: `PascalCase`

### Git Workflow

- Use descriptive branch names for features or fixes
- Commit messages should summarize changes in the imperative mood
- Open pull requests against the main branch and request review

## Environment Setup

### Development Requirements

- .NET 8.0 SDK
- Unity 2022 or newer
- Additional Rust toolchain for reference (optional)

### Installation Steps

```bash
# 1. Clone the project
git clone [repository-url]

# 2. Restore dependencies
# For .NET modules
dotnet restore VelorenPort/VelorenPort.sln

# 3. Open the Unity client
# Use Unity Hub or your preferred method
```

## Core Feature Implementation

### Feature Module 1

Example: world generation in `World`

```csharp
// Sample code
public class TerrainGenerator
{
    public Chunk Generate(Vector3 position)
    {
        // Noise-based generation similar to the Rust project
    }
}
```

### Feature Module 2

Example: server loop and networking in `Server`

## Testing Strategy

### Unit Testing

- Testing framework: xUnit
- Test coverage is encouraged but not enforced
- Place test files next to the code under `*.Tests` projects

### Integration Testing

- Focus on network protocol and world generation
- Use custom test runners or Unity Test Framework where applicable

### End-to-End Testing

- Manual gameplay testing in Unity client
- Automation planned with Unity Test Runner

## Deployment Guide

### Build Process

```bash
# Build the .NET solution
dotnet build VelorenPort/VelorenPort.sln
```

### Deployment Steps

1. Configure build settings in Unity and .NET
2. Set environment variables for server runtime
3. Package the client and server artifacts
4. Verify that the game launches and connects correctly

### Environment Variables

```env
API_URL=
DATABASE_URL=
SECRET_KEY=
```

## Performance Optimization

### Frontend Optimization

- Use Unity's profiling tools for frame rate analysis
- Implement asset caching and occlusion culling

### Backend Optimization

- Profile server tick rate and memory usage
- Cache heavy computations when possible

## Security Considerations

### Data Security

- Validate all network inputs
- Avoid SQL injection (when database integration is added)
- Sanitize user-generated content

### Authentication & Authorization

- Plan for account management and session tokens
- Enforce permissions for game actions

## Monitoring and Logging

### Application Monitoring

- Track server metrics with built-in logging
- Monitor Unity client performance

### Log Management

- Use standard logging frameworks (e.g., Microsoft.Extensions.Logging)
- Store logs per server instance for troubleshooting

## Common Issues

### Issue 1: Missing Unity packages

**Solution**: Open the project with Unity Hub to automatically resolve packages.

### Issue 2: Build errors on unsupported platforms

**Solution**: Check .NET and Unity versions; ensure all dependencies match the target platform.

## Reference Resources

- [Veloren Rust Repository](https://gitlab.com/veloren/veloren)
- [Unity Documentation](https://docs.unity3d.com)
- [Microsoft .NET Documentation](https://learn.microsoft.com/dotnet)

## Changelog

### v1.0.0 (2025-06-28)

- Initial template and planning documents
- Began porting core modules to C#

---

_Note: Adjust this document as development progresses and more specific instructions become necessary._
