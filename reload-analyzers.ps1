# Script para recargar los analizadores Roslyn en VS Code

Write-Host "`n1- Recargando analizadores DDD..." -ForegroundColor Cyan

# Limpiar y recompilar los analizadores
Write-Host "`n2- Limpiando proyecto de analizadores..." -ForegroundColor Yellow
dotnet clean DDD.Analyzers/DDD.Analyzers.csproj --verbosity quiet

Write-Host "`n3- Recompilando analizadores..." -ForegroundColor Yellow
dotnet build DDD.Analyzers/DDD.Analyzers.csproj --verbosity quiet

Write-Host "`n4- Recompilando proyecto de prueba..." -ForegroundColor Yellow
dotnet build TestDomain/TestDomain.csproj --verbosity quiet

Write-Host "`n¡Listo! Ahora:" -ForegroundColor Green
Write-Host "   1. Presiona Ctrl+Shift+P" -ForegroundColor White
Write-Host "   2. Busca: 'Developer: Reload Window'" -ForegroundColor White
Write-Host "   3. O cierra y abre VS Code" -ForegroundColor White
Write-Host "`nEsto recarga los analizadores y verás los warnings en tiempo real.`n" -ForegroundColor Cyan
