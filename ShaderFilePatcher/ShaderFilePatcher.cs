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
    const string ShadersPak = "game/csgo/shaders_pc_dir.vpk";
    const string Output = "patched_shaders/";

    const string DevShadersFolderV67 = @"D:\Users\kristi\Downloads\v67shaders\";
    const string DevShadersOutput = @"D:\Users\kristi\Downloads\v67shaders\output\";

    public static void Main(string[] args)
    {
        Console.WriteLine("CS2 Shader Patcher for Dota 2 Workshop Tools");

        if (!File.Exists(ShadersPak))
        {
            Console.WriteLine($"Failed to find shader package: {ShadersPak}");
            Console.ReadKey();
        }

        using var package = new Package();
        package.Read(ShadersPak);

        foreach (var entry in package.Entries["vcs"])
        {
            package.ReadEntry(entry, out var data);

            using var shaderFile = new ShaderFile();
            shaderFile.Read(entry.GetFullPath(), new MemoryStream(data));

            using var patchedShader = ShaderFile_V67_To_V66(shaderFile);
            var outPath = Path.Combine(Output, entry.GetFullPath());

            Console.WriteLine($"Patched shader: {entry.GetFullPath()}");
            ShaderFileSave(patchedShader, outPath);
        }

        foreach (var entry in package.Entries["ini"])
        {
            var outPath = Path.Combine(Output, entry.GetFullPath());
            File.WriteAllText(outPath, string.Empty);

            Console.WriteLine($"Shader {entry.GetFileName()} is ready!");
        }

        Console.WriteLine("Done generating shaders!");
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

        File.WriteAllBytes(filePath, stream.ToArray());
    }
}
