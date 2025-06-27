#!/bin/bash
set -e

# Publica el servidor en modo Release

dotnet publish Server.csproj -c Release -o build
