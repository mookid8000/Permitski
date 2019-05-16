using System;
using NUnit.Framework;
using Testy;

namespace Permitski.Tests
{
    [TestFixture]
    public class TestReadMeCode : FixtureBase
    {
        [Test]
        public void SignIt()
        {
            string json;

            var importantData = new ImportantData("hemmelig");

            var key = DocumentSigner.GenerateKey();
            using (var signer = new DocumentSigner(key))
            {
                var signed = signer.Sign(importantData);

                Console.WriteLine(signed);

                json = signed.ToString();
            }

            using (var signer = new DocumentSigner(key))
            {
                Console.WriteLine($"The document is valid: {signer.IsValid(json)}");
            }
        }

        class ImportantData
        {
            public string Data { get; }

            public ImportantData(string data)
            {
                Data = data;
            }
        }
    }
}