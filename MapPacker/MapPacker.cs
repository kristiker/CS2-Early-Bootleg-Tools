using ValveResourceFormat;
using SteamDatabase.ValvePak;
using System.Diagnostics;

if (args.Length == 0)
{
    Console.WriteLine("Please specify a '.vpk' map file.");
    Console.ReadKey();
    return;
}

var vpkFile = args[0];

var mod = Directory.GetParent(vpkFile);
var game = null as DirectoryInfo;
while (mod != null && mod.Name != "game")
{
    if (mod.Name == "maps")
    {
        mod = mod.Parent;
        game = mod;
        while (game != null && game.Name != "game")
        {
            game = game.Parent;
        }
        break;
    }

    mod = mod.Parent;
}

if (mod is null || game is null)
{
    Console.WriteLine("Map file is not inside the 'game/<mod_or_addon>/maps' folder. Please drag it from within that folder.");
    Console.ReadKey();
    return;
}

var vpkExe = Path.Combine(game.FullName, "bin", "win64", "vpk.exe");
if (!File.Exists(vpkExe))
{
    Console.WriteLine("Could not find 'vpk.exe' in the 'game/bin/win64/' folder.");
    Console.ReadKey();
    return;
}

var mapname = Path.GetFileNameWithoutExtension(vpkFile);

// Create a brand new folder to extract the map into
var mapFolder = Path.Combine(Environment.CurrentDirectory, $"{mapname}_pack")!;
if (Directory.Exists(mapFolder))
{
    Directory.Delete(mapFolder, true);
}

var package = new Package();
package.Read(vpkFile);

var inPackage = new HashSet<string>();

foreach (var entries in package.Entries.Values)
{
    foreach (var entry in entries)
    {
        var filePath = entry.GetFullPath();
        var extractFilePath = Path.Combine(mapFolder, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(extractFilePath)!);

        package.ReadEntry(entry, out var data);
        File.WriteAllBytes(extractFilePath, data);

        inPackage.Add(filePath);
        Console.WriteLine($"Extracted {filePath}");
    }
}

var vmap_path = Path.Combine(mapFolder, "maps", $"{mapname}.vmap_c");
var vmap_c = new Resource();
vmap_c.Read(vmap_path);
var new_files_added = 0;
CopyExternalReferences(vmap_c);

void CopyExternalReferences(Resource resource, int depth = 0)
{
    if (resource.ExternalReferences is null)
    {
        return;
    }

    foreach (var resource_reference in resource.ExternalReferences.ResourceRefInfoList)
    {
        var refFileName = resource_reference.Name + "_c";
        if (refFileName.StartsWith('_') || inPackage.Contains(refFileName))
        {
            continue;
        }

        var fullRefFilePath = Path.Combine(mod.FullName, refFileName);
        if (!File.Exists(fullRefFilePath))
        {
            Console.WriteLine($"Could not find '{refFileName}' in the mod folder.");
            continue;
        }

        for (var i = 0; i < depth; i++)
        {
            Console.Write("  ");
        }
        Console.WriteLine($"Found '{refFileName}' in the mod folder. Copying...");

        using var child_resource = new Resource();
        child_resource.Read(fullRefFilePath);
        CopyExternalReferences(child_resource, depth + 1);

        // Copy the file to the map folder
        var newFullFilePath = Path.Combine(mapFolder, refFileName);
        if (File.Exists(newFullFilePath))
        {
            continue;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(newFullFilePath)!);
        File.Copy(fullRefFilePath, newFullFilePath);
        new_files_added++;
    }
}

vmap_c.Dispose();

// Run vpk.exe to pack the map
var vpkProcess = new Process();
vpkProcess.StartInfo.FileName = vpkExe;
vpkProcess.StartInfo.Arguments = $"\"{mapFolder}\"";
vpkProcess.StartInfo.UseShellExecute = false;
vpkProcess.StartInfo.CreateNoWindow = true;
vpkProcess.Start();
vpkProcess.WaitForExit();

if (vpkProcess.ExitCode != 0)
{
    Console.WriteLine("Failed to pack the map with 'vpk.exe'.");
    Console.ReadKey();
    return;
}

// Rename file
var finalMap = Path.Combine(Environment.CurrentDirectory, mapname + ".vpk");
File.Move(mapFolder + ".vpk", finalMap, true);

Console.WriteLine($"Done! Packed {new_files_added} new files into the map file.");
Console.ReadKey();
