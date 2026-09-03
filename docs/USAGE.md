# Kullanım

## Altyazıları çıkarma

Bir `.mkv` dosyasına sağ tıkla ve **Export Subtitles With Saka** seçeneğine bas. Program MKV içindeki bütün altyazı parçalarını `A:\Anime` klasörüne çıkarır.

Komut satırından da çalıştırılabilir:

```powershell
SakaSubtitleExporter.exe --extract "A:\Anime\ornek.mkv"
```

## Dosya adları

Çıktı düzeni şöyledir:

```text
VideoAdı.Sıra.Dil.Başlık.uzantı
```

MKV içinde dil veya başlık bilgisi yoksa sırasıyla `und` ve `isimsiz` kullanılır. Dosya adında kullanılamayan karakterler alt çizgiye çevrilir.

## İşlem raporu

Her çalıştırmada kaynak videonun yanında değil, çıktı klasöründe bir rapor oluşturulur:

```text
VideoAdı._Saka-raporu.txt
```

Raporda bulunan, atlanan ve çıkarılamayan parçalar ayrı ayrı yazılır.

## Kaldırma

Windows'ta **Ayarlar > Uygulamalar > Yüklü uygulamalar** bölümünden **Saka Subtitle Exporter** öğesini kaldır. Sağ tık menüsü de kaldırma işlemiyle birlikte silinir.
