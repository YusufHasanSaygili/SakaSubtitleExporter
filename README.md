# Saka Subtitle Exporter

MKV dosyasının içindeki bütün altyazıları tek seferde çıkarmak için küçük bir Windows aracı. Arayüzü yok; Dosya Gezgini'ndeki sağ tık menüsünden çalışıyor.

## Kurulum

1. **Releases** bölümünden `SakaSubtitleExporterSetup.exe` dosyasını indir.
2. Kurulumu tamamla.
3. Bir MKV dosyasına sağ tıklayıp **Export Subtitles With Saka** seçeneğine bas.

Windows 11 menüyü doğrudan göstermiyorsa **Daha fazla seçenek göster** bölümüne bak.

Program ve sağ tık kısayolu yalnızca mevcut Windows kullanıcısı için kurulur. Yönetici izni istemez.

## Nereye çıkarıyor?

Bütün altyazıları `A:\Anime` klasörüne yazar. Dosya adında kaynak videonun adı, altyazı sıra numarası, dil ve parça başlığı bulunur.

Örnek:

```text
Bölüm 01.01.eng.Full Subtitles.ass
Bölüm 01.02.eng.Signs & Songs.ass
```

Aynı isimde bir dosya varsa üzerine yazmaz. İşlem sonunda klasörü açar ve `_Saka-raporu.txt` uzantılı bir işlem raporu bırakır.

## Desteklenen altyazılar

- ASS / SSA
- SRT / SubRip
- WebVTT
- PGS (`.sup`)
- Diğer metin altyazıları (SRT'ye dönüştürülür)
- Doğrudan dışarı aktarılamayan altyazılar (tek parçalı MKV olarak saklanır)

## Gerekenler

Windows 10 veya 11 ve FFmpeg gerekir. FFmpeg yüklü değilse PowerShell'de şu komutu çalıştırabilirsin:

```powershell
winget install Gyan.FFmpeg
```

## Kaynaktan derleme

Visual Studio 2022 veya .NET SDK 8 ve Inno Setup 6 gerekir.

```powershell
.\installer\Build-Setup.ps1
```

Hazır kurulum dosyası `artifacts\setup` klasörüne gelir. Ayrıntılar için [geliştirme notlarına](docs/DEVELOPMENT.md) bakabilirsin.
