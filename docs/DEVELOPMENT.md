# Geliştirme

## Yapı

- `src/SakaSubtitleExporter`: Windows uygulaması
- `tests/SakaSubtitleExporter.Tests`: bağımlılıksız birim testleri
- `installer`: Inno Setup paketi ve build betiği
- `.github/workflows`: Windows build ve artifact üretimi

## Derleme

```powershell
dotnet build .\SakaSubtitleExporter.sln -c Release
dotnet run --project .\tests\SakaSubtitleExporter.Tests -c Release --no-build
```

Kurulum paketini üretmek için Inno Setup 6 kurulu olmalı:

```powershell
winget install JRSoftware.InnoSetup
.\installer\Build-Setup.ps1
```

Build betiği önce çözümü ve testleri çalıştırır, ardından `artifacts\setup\SakaSubtitleExporterSetup.exe` dosyasını üretir.
