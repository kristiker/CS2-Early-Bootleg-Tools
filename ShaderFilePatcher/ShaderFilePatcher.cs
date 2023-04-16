using System;
using System.IO;
using System.Reflection;
using System.Text;
using SteamDatabase.ValvePak;
using ValveResourceFormat.CompiledShader;

namespace ShaderFilePatcher;

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

                var outPath = Path.Combine(Output, "v66", entry.GetFullPath());
                ShaderFileSave(patchedShader, outPath);
            }

            if (shaderFile == patchedShader)
            {
                Console.WriteLine($"Shader '{shaderFile.ShaderName}' is already version 66!");
            }

            if (shaderFile.VcsVersion == 66)
            {
                patchedShader = ShaderFile_BC7_To_DXT5(patchedShader, out var hasBC7);
                if (!hasBC7)
                {
                    Console.WriteLine($"Shader '{shaderFile.ShaderName}' does not reference BC7!");
                }
                else
                {
                    Console.WriteLine($"Patched shader (replaced BC7 refs with DXT5): {entry.GetFullPath()}");
                }

                var outPath = Path.Combine(Output, "v66_NoBC7", entry.GetFullPath());
                ShaderFileSave(patchedShader, outPath);
            }

            // Downgrading to v65 requires modifying compressed data.
            // Its just one additional byte though.
        }

        Console.WriteLine("Done patching shaders!");
        Console.ReadKey();
    }

    public static readonly PropertyInfo ShaderBlockOffset =
        typeof(ShaderDataBlock).GetProperty("Start", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static void PatchBytes(Stream stream, long blockOffset, int member, byte[] original, byte[] newValue)
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

    public static ShaderFile ShaderFile_V67_To_V66(ShaderFile shaderFile)
    {
        if (shaderFile.VcsVersion != 67)
        {
            throw new Exception($"Shader version is not 67! ({shaderFile.VcsVersion})");
        }

        var ms = CopyToNewMemoryStream(shaderFile.DataReader.BaseStream);

        // Change the version number
        if (shaderFile.FeaturesHeader != null)
        {
            var blockOffset = (long)ShaderBlockOffset.GetValue(shaderFile.FeaturesHeader)!;
            PatchBytes(ms, blockOffset, 4, BitConverter.GetBytes(67), BitConverter.GetBytes(66));
        }

        if (shaderFile.VspsHeader != null)
        {
            var blockOffset = (long)ShaderBlockOffset.GetValue(shaderFile.VspsHeader)!;
            PatchBytes(ms, blockOffset, 4, BitConverter.GetBytes(67), BitConverter.GetBytes(66));
        }

        // Change ChannelBlock.Channel value (only known difference in version 67)
        foreach (var channelBlock in shaderFile.ChannelBlocks)
        {
            var blockOffset = (long)ShaderBlockOffset.GetValue(channelBlock)!;
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

    public static ShaderFile ShaderFile_BC7_To_DXT5(ShaderFile shaderFile, out bool hasBC7)
    {
        if (shaderFile.VcsVersion < 66)
        {
            throw new Exception($"Shader version needs to be >= 66, actual: ({shaderFile.VcsVersion})");
        }

        var ms = CopyToNewMemoryStream(shaderFile.DataReader.BaseStream);

        if (shaderFile.VcsProgramType == VcsProgramType.Features)
        {
            var offset = FindSequenceIndex(ms.ToArray(), Encoding.ASCII.GetBytes("CsgoForward"));
            if (offset != -1)
            {
                PatchBytes(ms, offset, 0, Encoding.ASCII.GetBytes("CsgoForward"), Encoding.ASCII.GetBytes("DotaForward"));
                Console.WriteLine("Patched CsgoForward to DotaForward!");
            }
        }

        hasBC7 = false;
        foreach (var paramBlock in shaderFile.ParamBlocks)
        {
            if (paramBlock.ImageFormat != (int)ImageFormatV66.BC7)
            {
                continue;
            }

            var blockOffset = (long)ShaderBlockOffset.GetValue(paramBlock)!;
            var memberOffset = 64 + // Name
                               64 + // UiGroup
                               4 + // UiType
                               4 + // ...
                               64 + // AttributeName
                               4 + // ...
                               (paramBlock.DynExp.Length > 0 ? 4 : 0) +
                               paramBlock.DynExp.Length +
                               4 + // ...
                               (paramBlock.SBMSBytes.Length > 0 ? 8 : 0) + // ...
                               paramBlock.SBMSBytes.Length +
                               4 + // VfxType
                               4 + // ParamType
                               4 + // arg bytes
                               4 + // VecSize
                               4 + // arg bytes
                               64 + // FileRef
                               16 * 6 // Default values
                               ;

            PatchBytes(ms, blockOffset, memberOffset, BitConverter.GetBytes((int)ImageFormatV66.BC7), BitConverter.GetBytes((int)ImageFormatV66.DXT5));

            hasBC7 = true;
        }

        if (!hasBC7 && shaderFile.VcsProgramType != VcsProgramType.Features)
        {
            return shaderFile;
        }

        ms.Seek(0, SeekOrigin.Begin);

        var result = new ShaderFile();
        result.Read(shaderFile.FilenamePath, ms);

        return result;
    }

    private static int FindSequenceIndex(byte[] data, byte[] sequence)
    {
        for (int i = 0; i < data.Length - sequence.Length + 1; i++)
        {
            bool match = true;
            for (int j = 0; j < sequence.Length; j++)
            {
                if (data[i + j] != sequence[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                return i;
            }
        }
        return -1;
    }

    private static MemoryStream CopyToNewMemoryStream(Stream original)
    {
        var memoryStream = new MemoryStream();
        original.Seek(0, SeekOrigin.Begin);
        original.CopyTo(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public static void ShaderFileSave(ShaderFile shaderFile, string filePath)
    {
        var stream = CopyToNewMemoryStream(shaderFile.DataReader.BaseStream);

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
