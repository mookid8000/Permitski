using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Testy;

namespace Permitski.Tests;

[TestFixture]
public class TestInvariants : FixtureBase
{
    [Test]
    [Description("Verifies that a JSON-serialized complex object results in the exact same JSON output as it did when Permitski was initially made. This is important, because JSON is the basis of the signature, so it must be precise down to the last bit.")]
    public void TestJsonSerializationOfComplexObject()
    {
        var doc = new RootDocument("HEJ MED DIG", new[]
        {
            new ChildDocument("HEJ IGEN 1", new[] {1, 2, 3}),
            new ChildDocument("HEJ IGEN 2", new[] {1, 2, 3, 4, 5}),
            new ChildDocument("HEJ IGEN 3", new[] {1, 2, 3, 4, 5, 6, 7})
        });

        var json = JsonConvert.SerializeObject(doc);

        Console.WriteLine(json);

        Assert.That(json, Is.EqualTo(@"{""Text"":""HEJ MED DIG"",""Children"":[{""AnotherText"":""HEJ IGEN 1"",""ListOfIntegers"":[1,2,3]},{""AnotherText"":""HEJ IGEN 2"",""ListOfIntegers"":[1,2,3,4,5]},{""AnotherText"":""HEJ IGEN 3"",""ListOfIntegers"":[1,2,3,4,5,6,7]}]}"));
    }

    [Test]
    public void TestSignature()
    {
        var key = "Am/w5KeD4GEZzudvwucveBAHIxucC/cYNuEwFOFK4j0=|8rI4OAvVM4Opic0DrxEiVQ==";

        using var signer = new DocumentSigner(key);

        var signed = signer.Sign(new SimpleDocument("hej"));

        var json = signed.ToString(compact: true);

        Console.WriteLine(json);

        Assert.That(json, Is.EqualTo("{'Document':{'Text':'hej'},'Signature':'Ql8cwk1bFMjKCftWlj3K9NlD59c3Y0KoQMPnVcff/O8='}"));
    }

    record SimpleDocument(string Text);

    record RootDocument(string Text, IReadOnlyList<ChildDocument> Children);

    record ChildDocument(string AnotherText, IReadOnlyList<int> ListOfIntegers);
}