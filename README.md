# Rimworld Savegame Compressor

> Supports Rimworld `1.1` to `1.5`

The mod automatically compresses save files before writing them to disk, whether through manual saves or autosave. Compressed files are seamlessly decompressed in memory when loaded.

The size reduction can often be as much as 10x.

- Safe to add or remove mid-save, but make sure to decompress your saves if you want to be able to load them in vanilla.
- Additionally, the reduced file size helps extend your disk's lifespan by minimizing the amount of data being read and written.

Compression algorithm being used by the mod is `gzip`.

## Changes
- Minor changes made to the original mod to make it compatible with v1.5. 
- A new feature to compress/decompress all existing save files from settings.

## License[^1]
This library is free software; you can redistribute it and/or modify it under the terms of the [GNU Lesser General Public License](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.en.html) as published by the Free Software Foundation; either version 2.1 of the License, or (at your option) any later version.

[^1]: Licensing is based on the author’s note on the [workshop page.](https://steamcommunity.com/sharedfiles/filedetails/?id=2032036337)