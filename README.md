### ShaderRenamer
ShaderRenamer allows you to author Counter-Strike compatible materials in HL: Alyx. Simply run it once from within the Half-Life Alyx folder to generate the necessary shaders. The materials compiled using the new Csgo Complex "alias" are forward compatible with Counter-Strike 2.

### MapPacker
MapPacker will bundle required materials and textures with the map so it can be moved to Counter-Strike 2 as one package. Place this tool somewhere outside the game folder so it doesn't interfere with the original map files.

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
    - Csgo Simple - Enable AO+Metalness to fix dark checkerboards. Or ignore this shader and use Complex.
    - Csgo Static Overlay - No known fix for the purple checkerboards. Complex can be used for overlays by toggling translucent and overlay.
    
### Results
<a href="https://www.youtube.com/watch?v=Uf4zJCpWtI4">![image](https://user-images.githubusercontent.com/26466974/230385962-1596cfee-2c51-4fb7-84f6-1e14241f2284.png)</a>
![image](https://user-images.githubusercontent.com/26466974/232033473-73c1422e-b51c-474d-9e17-77b91a23f11d.png)
![image](https://user-images.githubusercontent.com/26466974/232033892-2cf16cb7-3000-47c7-ac7d-bf77b78b8c9c.png)
