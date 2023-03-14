using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Permitski.Tests;

[TestFixture]
public class TestAssumptionsAboutJsonSerializer
{
    [Test]
    public void CanOmitNullProperties()
    {
        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        var json = JsonConvert.SerializeObject(new MyLicenseDocument("John Doe", "20230314"), settings);

        Assert.That(json, Is.EqualTo("{\"Name\":\"John Doe\",\"IssuedOn\":\"20230314\"}"));
    }

    [Test]
    public void OmitsNullFieldsWhenGeneratingSignedDocuments()
    {
        var key = DocumentSigner.GenerateKey();

        using var signer = new DocumentSigner(key);

        var signedWithoutExpiry = signer.Sign(new MyLicenseDocument("John Doe", "20230314"));
        var signedWithExpiry = signer.Sign(new MyLicenseDocument("John Doe", "20230314", "20260401"));

        Console.WriteLine(signedWithoutExpiry.ToString());
        Console.WriteLine(signedWithExpiry.ToString());

        int CountExpiryProperties(Signed<MyLicenseDocument> signed)
        {
            var json = signed.ToString();
            var obj = JObject.Parse(json);
            var document = obj["Document"] as JObject ?? throw new InvalidCastException($"Could not case {obj["Document"]} into JObject");
            return document.Properties().Count(p => p.Name == "Expiry");
        }

        Assert.That(CountExpiryProperties(signedWithoutExpiry), Is.EqualTo(0));
        Assert.That(CountExpiryProperties(signedWithExpiry), Is.EqualTo(1));
    }

    public record MyLicenseDocument(string Name, string IssuedOn, string Expiry = null);
}