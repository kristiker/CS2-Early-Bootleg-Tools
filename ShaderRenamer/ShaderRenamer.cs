using SteamDatabase.ValvePak;

Console.WriteLine("CS2 Shader Generator for Half-Life: Alyx Workshop Tools");

var shadersToExtract = new[]
{
    "vr_simple",
    "vr_complex",
    "vr_black_unlit",
    "vr_simple_2way_blend",
    "vr_static_overlay",
};

var shaderFileSuffixes = new[]
{
    "features",
    "vs",
    "ps",
    "hs",
    "ds",
    "cs",
    "gs",
    "rtx",
    "psrs",
};

var shadersPak = "game/hlvr/shaders_pc_dir.vpk";

if (!File.Exists(shadersPak))
{
    Console.WriteLine($"Failed to find shader package: {shadersPak}");
    Console.ReadKey();
    return;
}

using var package = new Package();
package.Read(shadersPak);

foreach (var vr_name in shadersToExtract)
{
    var ini = package.FindEntry($"shaders/vfx/{vr_name}.ini");
    if (ini is null)
    {
        Console.WriteLine($"Failed to find shader with name: {vr_name}");
        continue;
    }

    // Replace vr_ with csgo_
    var csgo_name = "csgo_" + vr_name[3..];

    if (!Directory.Exists("game/hlvr/shaders/vfx"))
    {
        Directory.CreateDirectory("game/hlvr/shaders/vfx");
    }

    File.WriteAllText($"game/hlvr/shaders/vfx/{csgo_name}.ini", string.Empty);

    foreach (var suffix in shaderFileSuffixes)
    {
        var file = package.FindEntry($"shaders/vfx/{vr_name}_pc_50_{suffix}.vcs");
        if (file is not null)
        {
            package.ReadEntry(file, out var data);
            File.WriteAllBytes($"game/hlvr/shaders/vfx/{csgo_name}{file.FileName[vr_name.Length..]}.vcs", data);
        }
    }

    Console.WriteLine($"+ Generated shader {csgo_name}");
}

Console.WriteLine("Done generating shaders!");
Console.ReadKey();
