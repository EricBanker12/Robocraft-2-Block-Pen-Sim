using System.Text;

// Still requires too many dll files in exe folder, and this method is likely to cause bugs/crashes on other platforms.
// Better to stick with a launcher instead.

var exePath = args[0];
if (Path.GetExtension(exePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
{
    var exeDir = Path.GetDirectoryName(exePath) ?? string.Empty;
    var exeName = Path.GetFileName(exePath);
    var exePatchedName = Path.GetFileNameWithoutExtension(exeName) + ".patched.exe";
    var exePatchedPath = Path.Combine(exeDir, exePatchedName);
                
    var bin = "bin";
    var binPath = Path.Combine(exeDir, bin);
    if (!Directory.Exists(binPath)) Directory.CreateDirectory(binPath);

    var dllName = Path.ChangeExtension(exeName, ".dll");
    var bDllName = Encoding.UTF8.GetBytes(dllName);
                
    var depPreName = "<asmv3:file name='";
    var bDepPreName = Encoding.UTF8.GetBytes(depPreName);
                
    using (var exeStream = new FileStream(exePath, FileMode.Open))
    {
        using (var patchedStream = new FileStream(exePatchedPath, FileMode.Create))
        {
            var depDllNameExclusions = new string[]
            {
                "CoreMessagingXP.dll",
                "dcompi.dll",
                "Microsoft.UI.Input.dll",
            };

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
                        await patchedStream.WriteAsync(Encoding.UTF8.GetBytes(Path.Combine(bin, dllName)));
                        b = exeStream.ReadByte();
                        b = exeStream.ReadByte();
                        b = exeStream.ReadByte();
                        b = exeStream.ReadByte();
                        b = exeStream.ReadByte();
                    }
                    else
                    {
                        exeStream.Position -= posAhead;
                    }
                }

                if (b == bDepPreName[0])
                {
                    var matched = true;
                    var posAhead = 0;
                    for (int i = 1; i < bDepPreName.Length && matched; i++)
                    {
                        matched = bDepPreName[i] == exeStream.ReadByte();
                        posAhead++;
                    }

                    if (matched)
                    {
                        var sb = new StringBuilder();
                        var d = exeStream.ReadByte();
                        posAhead++;
                        while (d > 0 && d != 0x27)
                        {
                            sb.Append(Encoding.UTF8.GetString(new byte[] { (byte)d }));
                            d = exeStream.ReadByte();
                            posAhead++;
                        }
                        var depDllName = sb.ToString();
                        if (depDllNameExclusions.Contains(depDllName))
                        {
                            exeStream.Position -= posAhead;
                        }
                        else
                        {
                            await patchedStream.WriteAsync(Encoding.UTF8.GetBytes($"{depPreName}{Path.Combine(bin, depDllName)}'"));

                            b = exeStream.ReadByte();
                            while(b > 0 && b != 0x20)
                            {
                                patchedStream.WriteByte((byte)b);
                                b = exeStream.ReadByte();
                            }
                            b = exeStream.ReadByte();
                            b = exeStream.ReadByte();
                            b = exeStream.ReadByte();
                            b = exeStream.ReadByte();
                        }
                    }
                    else
                    {
                        exeStream.Position -= posAhead;
                    }
                }

                if (b >= 0)
                {
                    patchedStream.WriteByte((byte)b);
                }
                else
                {
                    break;
                }
            }
        }
    }
}