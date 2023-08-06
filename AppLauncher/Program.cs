using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AppLauncher
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        static void Main(string[] args)
        {
            var targetPath = Path.Join(AppContext.BaseDirectory, "Robocraft2BlockPenSimApp.exe");

            try
            {
                string hash;
                using (var fs = new FileStream(targetPath, FileMode.Open))
                {
                    using (var sha = SHA256.Create())
                    {
                        hash = string.Join("", sha.ComputeHash(fs).Select(b => b.ToString("X2")));
                    }
                }

                var targetHash = "C609C45BBBF9D077A2EA5722B78B4BAC6C330FFCE24FC46DF521C366E3296B6E";
                if (hash == targetHash)
                {
                    Process.Start(targetPath);
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("The sha256 hash for Robocraft2BlockPenSimApp.exe did not match");
                    sb.AppendLine($"Received: {hash}");
                    sb.AppendLine($"Expected: {targetHash}");
                    sb.AppendLine("Please reinstall Robocraft2BlockPenSimApp to fix this issue: https://github.com/EricBanker12/Robocraft-2-Block-Pen-Sim/releases/latest");
                    throw new ApplicationException(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                AllocConsole();
                if (ex is ApplicationException)
                {
                    Console.WriteLine(ex.Message);
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.WriteLine($"Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}