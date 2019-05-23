using System;
using NUnit.Framework;
using Testy;

namespace Permitski.Tests
{
    [TestFixture]
    public class TestPermitski : FixtureBase
    {
        [Test]
        public void CanSignEmptyDocument()
        {
            var signer = new DocumentSigner(DocumentSigner.GenerateKey());

            Using(signer);

            var emptyDocument = new { };

            var signedDocument = signer.Sign(emptyDocument);

            Console.WriteLine($@"Here's the signed empty document:

{signedDocument}");

            Assert.That(signedDocument.Document, Is.EqualTo(emptyDocument));
            Assert.That(signedDocument.Signature, Is.Not.Empty);
        }

        [Test]
        public void GetsSameSignatureWhenUsingSameKey()
        {
            var key = DocumentSigner.GenerateKey();

            var signer1 = Using(new DocumentSigner(key));
            var signer2 = Using(new DocumentSigner(key));

            var document = new { Text = "hej med dig min søde ven" };

            Assert.That(signer1.Sign(document).Signature, Is.EqualTo(signer2.Sign(document).Signature));
        }

        [Test]
        public void GetsDifferentSignaturesWhenUsingDifferentData()
        {
            var key = DocumentSigner.GenerateKey();

            var signer = Using(new DocumentSigner(key));

            var firstDocument = new { Text = "hej" };
            var secondDocument = new { Text = "Hej" };

            Assert.That(signer.Sign(firstDocument).Signature, Is.Not.EqualTo(signer.Sign(secondDocument).Signature));
        }

        [Test]
        public void CanCheckSignature_Positive()
        {
            var key = DocumentSigner.GenerateKey();
            var signer = Using(new DocumentSigner(key));
            var document = new { Text = "okkergokkergummiklokker" };

            var signed = signer.Sign(document);

            Assert.That(signer.IsValid(signed), Is.True);
        }

        [Test]
        public void CanCheckSignature_Negative()
        {
            var signer1 = Using(new DocumentSigner(DocumentSigner.GenerateKey()));
            var signer2 = Using(new DocumentSigner(DocumentSigner.GenerateKey()));
            var document = new { Text = "okkergokkergummiklokker" };

            var signed2 = signer2.Sign(document);

            Assert.That(signer1.IsValid(signed2), Is.False);
        }

        [Test]
        public void CanCheckSignature_Json_Positive()
        {
            var key = DocumentSigner.GenerateKey();
            var signer = Using(new DocumentSigner(key));
            var document = new { Text = "okkergokkergummiklokker" };

            var signed = signer.Sign(document);

            var json = signed.ToString();

            Console.WriteLine($@"Checking this JSON:

{json}");

            Assert.That(signer.IsValid(json), Is.True);
        }

        [Test]
        public void CanCheckSignature_Json_Negative()
        {
            var signer1 = Using(new DocumentSigner(DocumentSigner.GenerateKey()));
            var signer2 = Using(new DocumentSigner(DocumentSigner.GenerateKey()));
            var document = new { Text = "okkergokkergummiklokker" };

            var signed2 = signer2.Sign(document);

            Assert.That(signer1.IsValid(signed2.ToString()), Is.False);
        }

        [TestCase(@"

{
  ""Document"": {
    ""Text"": ""okkergokkergummiklokker""
  },
  ""Signature"": ""l8RlgY4WZF0iDfgOh3ftCgfr/eicN2ltBEhOXhPTlIs=""
}

")]
        [TestCase(@"

{
  ""Signature"": ""l8RlgY4WZF0iDfgOh3ftCgfr/eicN2ltBEhOXhPTlIs="",
  ""Document"": {
    ""Text"": ""okkergokkergummiklokker""
  }
}

")]
        [TestCase(@"{""Signature"": ""l8RlgY4WZF0iDfgOh3ftCgfr/eicN2ltBEhOXhPTlIs="",""Document"": {""Text"": ""okkergokkergummiklokker""}}")]
        public void CanCheckSignature_Json_Reformatting(string jsonDocumentWithValidSignature)
        {
            const string key = "TNm1TDf3V5lLHZRFU3rGmC+9u3pIfuOOB0tmTpWXvLQ=|S+UfLmShvBPL56JkdgKyFg==";

            var signer = Using(new DocumentSigner(key));

            Console.WriteLine($@"Checking this variation:

{jsonDocumentWithValidSignature}");

            Assert.That(signer.IsValid(jsonDocumentWithValidSignature), Is.True);
        }
    }
}
