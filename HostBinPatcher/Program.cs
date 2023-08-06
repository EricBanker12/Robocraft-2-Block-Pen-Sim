using System.Text;

var exePath = args[0];
if (Path.GetExtension(exePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
{
    var exeDir = Path.GetDirectoryName(exePath) ?? string.Empty;
    var exeName = Path.GetFileName(exePath);
    var pdbName = Path.ChangeExtension(exeName, ".pdb");
    var unpatchedExeName = Path.GetFileNameWithoutExtension(exeName) + ".Unpatched.exe";
    var unpatchedExePath = Path.Combine(exeDir, unpatchedExeName);
                
    var bin = "bin";
    var binPath = Path.Combine(exeDir, bin);
    if (!Directory.Exists(binPath)) Directory.CreateDirectory(binPath);

    var dllName = Path.ChangeExtension(exeName, ".dll");
    var bDllName = Encoding.UTF8.GetBytes(dllName);
    var patchedDllName = Path.Combine(bin, dllName);
    var bPatchedDllName = Encoding.UTF8.GetBytes(patchedDllName);

    using (var exeStream = new FileStream(exePath, FileMode.Open))
    {
        using (var unpatchedStream = new FileStream(unpatchedExePath, FileMode.CreateNew))
        {
            while (true)
            {
                var b = exeStream.ReadByte();
                if (b == bDllName[0])
                {
                    var matched = true;
                    var posAhead = 0;
                    for (int i = 1; i < bDllName.Length && matched; i++)
                    {
                        matched = bDllName[i] == exeStream.ReadByte();
                        posAhead++;
                    }

                    if (matched)
                    {
                        var buffer = new byte[bPatchedDllName.Length];
                        
                        exeStream.Position -= bDllName.Length;
                        await exeStream.ReadAsync(buffer);
                        await unpatchedStream.WriteAsync(buffer);
                        
                        exeStream.Position -= bPatchedDllName.Length;
                        await exeStream.WriteAsync(bPatchedDllName);
                        break;
                    }
                    else
                    {
                        exeStream.Position -= posAhead;
                    }
                }

                if (b >= 0)
                {
                    unpatchedStream.WriteByte((byte)b);
                }
                else
                {
                    break;
                }
            }
            if (exeStream.Position < exeStream.Length)
            {
                await exeStream.CopyToAsync(unpatchedStream);
            }
        }
    }

    foreach (var file in new DirectoryInfo(exeDir).GetFiles())
    {
        if (file.Name != exeName && file.Name != unpatchedExeName && file.Name != pdbName)
            file.MoveTo(Path.Combine(binPath, file.Name));
    }
}
