using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Security.Cryptography;
using Testy;

namespace Permitski.Tests;

[TestFixture]
[Explicit("Just play aroudn with it")]
public class TestAsymmetricCrypto : FixtureBase
{
    [Test]
    public void CanDoIt()
    {
        using var rsa = RSA.Create();

        rsa.KeySize = 16384;

        
        rsa.ImportRSAPrivateKey(GetKeyFromFile("testkey.key"), out _);
    }

    static ReadOnlySpan<byte> GetKeyFromFile(string fileName)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, fileName))
            .SkipWhile(line => line.Trim().StartsWith("-----BEGIN ") && line.Trim().EndsWith("-----"))
            .TakeWhile(line => !(line.StartsWith("-----END") && line.EndsWith("-----")));

        var text = string.Join("", lines);

        Console.WriteLine(text);

        return Convert.FromBase64String(text);
    }
}