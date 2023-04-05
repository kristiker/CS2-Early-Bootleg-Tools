### ShaderRenamer
ShaderRenamer allows you to author Counter-Strike compatible materials in HL: Alyx. Simply run it once from within the Half-Life Alyx folder to generate the necessary shaders. The materials compiled using these new "aliases" can be used in Counter-Strike 2 maps with few caveats.

### MapPacker
MapPacker will bundle required materials and textures with the map so it can be moved to Counter-Strike 2 as one package. It is best if the materials in the map use the aforementioned shaders.

### gameinfo.gi
Contains the necessary edits to trick CS2 into thinking the lightmaps are of the latest version. Backup your game/hlvr/gameinfo.gi and replace it with this one. 

#### Workflow

1. Use ShaderRenamer to generate required shaders. Replace gameinfo.gi.
2. Create new Csgo Complex materials, or port materials from Source with [source1import](https://github.com/kristiker/source1import).
3. Compile your map in your HLVR addon. You can test it in HLVR.
4. To test the map in CS2, drag the HLVR compiled vpk file over to MapPacker. Copy the new VPK to CS2.
5. If you don't replace the gameinfo.gi file, CS2 will warn about outdated lightmaps and render fullbright.

### Problems

1. In some areas there is flickering void - To fix disable vis for your map.
    - todo. vmap can be [compiled](/content/dota_addons/cs2/maps/COMPILEVIS3.cmd) in dota to get VIS3 worldnode and vvis_c.
3. Checkerboard on some materials
    - Csgo Simple - To fix enable Ambient Occlusion texture even if there is no AO. Or ignore this shader and use Complex.
    - Csgo Static Overlay - No known fix for the purple checkerboards.
