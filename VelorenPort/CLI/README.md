# CLI

Herramientas de consola y utilidades para el servidor (`server-cli`).

**Viabilidad**: Alta. Las aplicaciones de consola son sencillas de portar a C#.

**Notas**:
- Reescribir comandos y utilidades usando .NET `System.CommandLine` o similar.
- Comandos disponibles:
  - `admin add <usuario> <rol>` / `admin remove <usuario>`
  - `ban add <usuario> <razón>` / `ban remove <usuario>`
  - `whitelist add <usuario>` / `whitelist remove <usuario>`
