# Saka artwork

- `saka.svg` is the editable cat-and-fish icon.
- `saka.png` is the transparent 512-pixel raster version.
- `saka.ico` contains 16, 24, 32, 48, 64, 128 and 256-pixel sizes for Windows.
- `mascot.png` is the approved character illustration with a real alpha channel. Its exterior white background was removed by a border-connected flood fill, with white-matte cleanup limited to a two-pixel edge band. Interior colors and white clothing details are retained.
- `readme-mascot.png` is a separate README illustration: the same character looks right, curls her hands like cat paws, and raises one foot behind her. It does not replace the in-app mascot or the application icon.

The character was generated and refined with user direction. The final background removal was performed locally with code, not by regenerating the character. The paw and fishbone pattern is a tiled WPF drawing in `MainWindow.xaml`.
