// ═══════════════════════════════════════════════════════════════════════════
//  Smart Pharmacy – Developer License Keygen Tool
//  Run this locally (NEVER ship it with the product) to generate license keys.
//
//  Usage:
//    1. dotnet run   — interactive prompt
//    2. dotnet run -- <MACHNE-ID>  — non-interactive (CI / scripts)
//
//  Build a self-contained executable (Windows x64):
//    dotnet publish -c Release -r win-x64 --self-contained true
// ═══════════════════════════════════════════════════════════════════════════

using System.Security.Cryptography;
using System.Text;

// ─── SECRET SALT ─────────────────────────────────────────────────────────────
// ⚠️  MUST match the value in LicenseService.cs exactly.
// ⚠️  Keep this file private — never push it to a public repository.
const string SecretSalt = "SP@SmartPharmacy#2025!SecureKey$";

// ─── Banner ──────────────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║   Smart Pharmacy – License Key Generator             ║");
Console.WriteLine("║   Developer Tool — DO NOT DISTRIBUTE                 ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

// ─── Get Machine ID ──────────────────────────────────────────────────────────
string machineId;

if (args.Length > 0)
{
    // Non-interactive mode: accept Machine ID as CLI argument
    machineId = args[0].Trim();
    Console.WriteLine($"Machine ID (from args): {machineId}");
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("  Enter the client Machine ID (e.g. ABCD-1234-EFGH-5678): ");
    Console.ResetColor();
    machineId = (Console.ReadLine() ?? string.Empty).Trim();
}

// ─── Validate Format ─────────────────────────────────────────────────────────
if (string.IsNullOrWhiteSpace(machineId))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  ✗  Error: Machine ID cannot be empty.");
    Console.ResetColor();
    Environment.Exit(1);
}

// Expected format: XXXX-XXXX-XXXX-XXXX (16 hex + 3 dashes = 19 chars)
var parts = machineId.Split('-');
if (parts.Length != 4 || parts.Any(p => p.Length != 4))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  ✗  Warning: Machine ID does not match expected format XXXX-XXXX-XXXX-XXXX.");
    Console.WriteLine("     Proceeding anyway — make sure this ID came from the pharmacy system.");
    Console.ResetColor();
    Console.WriteLine();
}

// ─── Generate License Key ─────────────────────────────────────────────────────
var licenseKey = ComputeHmacSha256(machineId, SecretSalt);

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("  ✔  License Key Generated Successfully!");
Console.ResetColor();
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"  Machine ID  : {machineId}");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"  License Key : {licenseKey}");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine("  Copy the License Key above and send it to the client.");
Console.WriteLine("  They must enter it in the Lock Screen of the pharmacy system.");
Console.WriteLine();

if (args.Length == 0)
{
    Console.WriteLine("  Press any key to exit...");
    Console.ReadKey();
}

// ─── Helper ──────────────────────────────────────────────────────────────────
static string ComputeHmacSha256(string data, string key)
{
    var keyBytes  = Encoding.UTF8.GetBytes(key);
    var dataBytes = Encoding.UTF8.GetBytes(data);
    using var hmac = new HMACSHA256(keyBytes);
    return Convert.ToBase64String(hmac.ComputeHash(dataBytes));
}
