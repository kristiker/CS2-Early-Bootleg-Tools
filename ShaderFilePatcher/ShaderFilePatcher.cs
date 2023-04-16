using System;
using System.IO;
using System.Reflection;
using SteamDatabase.ValvePak;
using ValveResourceFormat.CompiledShader;

namespace ShaderFilePatcher;

public enum Game
{
    Dota,
    Steampal,
}

public class ShaderPatcher
{
    const string Output = "patched_shaders/";

    public static void Main(string[] args)
    {
        Console.WriteLine("CS2 Shader Patcher for Dota 2 Workshop Tools");

        if (args.Length == 0)
        {
            Console.WriteLine("Please drag over the 'shaders_pc_dir.vpk' file.");
            Console.ReadKey();
            return;
        }

        var shadersPak = args[0];

        if (!File.Exists(shadersPak))
        {
            Console.WriteLine($"Failed to find shader package: {shadersPak}");
            Console.ReadKey();
        }

        using var package = new Package();
        package.Read(shadersPak);

        if (!package.IsDirVPK)
        {
            Console.WriteLine($"Note: Shader package {Path.GetFileName(shadersPak)} is not the dir VPK!");
        }

        foreach (var entry in package.Entries["vcs"])
        {
            package.ReadEntry(entry, out var data);

            using var shaderFile = new ShaderFile();
            shaderFile.Read(entry.GetFullPath(), new MemoryStream(data));

            var patchedShader = shaderFile;

            if (shaderFile.VcsVersion > 66)
            {
                patchedShader = ShaderFile_V67_To_V66(patchedShader);
                Console.WriteLine($"Patched shader {shaderFile.VcsVersion}->{patchedShader.VcsVersion}: {entry.GetFullPath()}");

                var outPath = Path.Combine(Output, entry.GetFullPath());
                ShaderFileSave(patchedShader, outPath);
            }

            if (shaderFile == patchedShader)
            {
                Console.WriteLine($"Shader {entry.FileName} is already version 66!");
            }


        }

        Console.WriteLine("Done patching shaders!");
        Console.ReadKey();
    }


    public static ShaderFile ShaderFile_V67_To_V66(ShaderFile shaderFile)
    {
        if (shaderFile.VcsVersion != 67)
        {
            throw new Exception($"Shader version is not 67! ({shaderFile.VcsVersion})");
        }

        var shaderDataBlockStart = typeof(ShaderDataBlock)
            .GetProperty("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        var ms = new MemoryStream();
        shaderFile.DataReader.BaseStream.Position = 0;
        shaderFile.DataReader.BaseStream.CopyTo(ms);

        static void PatchBytes(Stream stream, long blockOffset, int member, byte[] original, byte[] newValue)
        {
            if (original.Length != newValue.Length)
            {
                throw new Exception($"New value size doesn't match! {newValue.Length} != {original.Length}");
            }

            Console.WriteLine(
                $"\tblock: {blockOffset}, member: 0x{member:X}, size: 0x{original.Length:X}\n" +
                $"\t\torig:\t{BitConverter.ToString(original)}\n\t\tnew:\t{BitConverter.ToString(newValue)}");

            stream.Seek(blockOffset + member, SeekOrigin.Begin);

            foreach (var byteToCompare in original)
            {
                var actual = stream.ReadByte();
                if (actual != byteToCompare)
                {
                    throw new Exception($"Byte is different! {actual:X} != {byteToCompare:X}");
                }
            }

            stream.Seek(blockOffset + member, SeekOrigin.Begin);
            stream.Write(newValue, 0, newValue.Length);
        }

        // Change the version number
        if (shaderFile.FeaturesHeader != null)
        {
            var blockOffset = (long)shaderDataBlockStart!.GetValue(shaderFile.FeaturesHeader)!;
            PatchBytes(ms, blockOffset, 4, BitConverter.GetBytes(67), BitConverter.GetBytes(66));
        }

        if (shaderFile.VspsHeader != null)
        {
            var blockOffset = (long)shaderDataBlockStart!.GetValue(shaderFile.VspsHeader)!;
            PatchBytes(ms, blockOffset, 4, BitConverter.GetBytes(67), BitConverter.GetBytes(66));
        }

        // Change ChannelBlock.Channel value (only known difference in version 67)
        foreach (var channelBlock in shaderFile.ChannelBlocks)
        {
            var blockOffset = (long)shaderDataBlockStart!.GetValue(channelBlock)!;
            PatchBytes(ms,
                blockOffset,
                0x0,
                BitConverter.GetBytes(channelBlock.Channel.PackedValue),
                (byte[])channelBlock.Channel.Channels
            );
        }

        if (shaderFile.DataReader.BaseStream.Length != ms.Length)
        {
            throw new Exception(
                "Something went wrong! Sizes don't match! " +
                $"{shaderFile.DataReader.BaseStream.Length} != {ms.Length}, block count: {shaderFile.ChannelBlocks.Count}"
            );
        }

        ms.Seek(0, SeekOrigin.Begin);

        var result = new ShaderFile();
        result.Read(shaderFile.FilenamePath, ms);

        return result;
    }

    public static void ShaderFileSave(ShaderFile shaderFile, string filePath)
    {
        var stream = new MemoryStream();
        shaderFile.DataReader.BaseStream.Position = 0;
        shaderFile.DataReader.BaseStream.CopyTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var outDir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir!);
        }

        var ini = Path.Combine(outDir!, shaderFile.ShaderName + ".ini");
        if (!File.Exists(ini))
        {
            File.WriteAllText(ini, string.Empty);
        }

        File.WriteAllBytes(filePath, stream.ToArray());
    }
}
