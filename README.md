# Permitski

A simple .NET library for generating crypgraphically signed documents.

First you generate a key

```csharp
var key = DocumentSigner.GenerateKey();
```

which you might want to hold on to, because it's your secret. 

If you lose the key, you lose the ability to sign documents and validate signatures of signed documents, so you can probably imagine that it's pretty important.

With the key in hand, you can create a signer like this:
```csharp
using(var signer = new DocumentSigner(key))
{
	// sign & validate stuff in here
}
```

Since it's relying on unmanaged resources some place deep within the bowels of the .NET framework, you want to make sure that you dispose the signer properly.

Now, let's pretend we have some important data we want to pass around with a signature, so we can validate its authenticity later on. Here's the class that represents our important data:

```csharp
class ImportantData
{
    public string Data { get; }

    public ImportantData(string data)
    {
        Data = data;
    }
}
```

Let's sign it, so we can give it to someone:
```csharp
var importantData = new ImportantData("hemmelig");

var signed = signer.Sign(importantData);

Console.WriteLine(signed);
```

which in this case yielded the following output:

```json
{
  "Document": {
    "Data": "hemmelig"
  },
  "Signature": "YHZxo95/CfP4P/ndfh+49m5kmjb2Zj8xkGrkvKVFbcw="
}
```

Let's see if it's valid - if we store the JSON shown above in a string called `json`, we can check its validity like this:

```csharp
Console.WriteLine($"The document is valid: {signer.IsValid(json)}");
```
which on my machine yielded:
```
The document is valid: True
```

Neat!

