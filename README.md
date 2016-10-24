# Arcanum Texture Slicer

The tool is created for [Another Arcanum Project](http://another-arcanum.ru) in order to easily split large textures into rhomboid tiles. Textures are stored as 8bit bmp files where the first color in the palette is treated as trasparent. The resulting tiles have the same format and color table as the initial texture.

## System Requirements
- Windows 7 or later
- .Net Framework 4.5

## Console Version
```
ArcanumTextureSlicer.Console.exe input-file output-folder x-offset y-offset
```
- `input-file` - path to the initial 8bit bmp texture, required.
- `output-folder` - path to a folder where the resulting tile are saved, required.
- `x-offset` - horizontal offset in pixels of the first tile from the top left corner of the input image, optional, 0 by default.
- `y-offset` - vartical offset in pixels of the first tile from the top left corner of the input image, optional, 0 by default.

The program locates the first tile and exports all tiles to the right and down from it. First tile can be determined different ways (in order of priority):

1. Using optional command line arguments.
2. Having a black (0x00) tile on the input image (assuming that the transparent color is not black).
3. Default 0,0 coordinates.

## GUI Version
```
ArcanumTextureSlicer.Gui.exe input-file
```
- `input-file` - file to load initially, optional.

GUI version allows to visually move the grid of tiles on the input image and select desired tiles to export. Tiles are exported in order of selection.

### File Options
- `Ctrl + O` - open bmp file.
- `Ctrl + E` - export selected tiles.

### Image Options
- `Left Mouse + Drag` - move the image in the viewport.
- `Ctrl + Mouse Wheel` - zoom the image.
- `Ctrl + 1,2,3,4` - zoom the image with keyboard.

### Grid Options
- `Ctrl + Up,Down,Left,Right` - move the grid 1px to the selected direction.
- `Ctrl + Shift + Up,Down,Left,Right` - move the grid 10px to the selected direction.
- `Ctrl + Left Mouse` - select a tile to export.
