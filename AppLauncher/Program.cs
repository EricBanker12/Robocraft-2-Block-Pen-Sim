using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;

namespace AppLauncher
{
    internal class Program
    {
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
                    Console.WriteLine("The sha256 hash for Robocraft2BlockPenSimApp.exe did not match.");
                    Console.WriteLine($"Received: {hash}");
                    Console.WriteLine($"Expected: {targetHash}");
                    Console.WriteLine("Please reinstall Robocraft2BlockPenSimApp to fix this issue: https://github.com/EricBanker12/Robocraft-2-Block-Pen-Sim/releases/latest");
                    Console.WriteLine($"Press any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}