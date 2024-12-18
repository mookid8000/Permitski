﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Permitski.Internals;

namespace Permitski;

/// <summary>
/// Use this to perform the necessary actions for creating signed documents and validating existing ones.
/// </summary>
public class DocumentSigner : IDisposable
{
    /// <summary>
    /// Cann this to generate a new signing key. The key is symmetric, so you need this key both for signing and validating.
    /// </summary>
    public static string GenerateKey() => SymmetricCrypto.GenerateNewKey();

    static readonly JsonSerializerSettings SerializerSettings = new() { NullValueHandling = NullValueHandling.Ignore };

    readonly SymmetricAlgorithm _cryptoServiceProvider;

    bool _disposed;
    static readonly string SignatureFieldName = nameof(Signed<object>.Signature);
    static readonly string DocumentFieldName = nameof(Signed<object>.Document);

    /// <summary>
    /// Creates the <see cref="DocumentSigner"/> with the given <paramref name="key"/>. Call <see cref="GenerateKey"/> if you need a new key.
    /// </summary>
    public DocumentSigner(string key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        _cryptoServiceProvider = SymmetricCrypto.GetFromKey(key);
    }

    /// <summary>
    /// Returns the <paramref name="document"/> wrapped in a <see cref="Signed{T}"/> containing the signature.
    /// </summary>
    public Signed<TDocument> Sign<TDocument>(TDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        return new Signed<TDocument>(document, GenerateSignatureFromObject(document));
    }

    /// <summary>
    /// Deserializes (but DOESN'T VALIDATE!) the serialized signed document <paramref name="json"/> into a <see cref="Signed{T}"/> containing <typeparamref name="TDocument"/>
    /// </summary>
    public Signed<TDocument> Deserialize<TDocument>(string json)
    {
        Signed<TDocument> Parse()
        {
            try
            {
                return JsonConvert.DeserializeObject<Signed<TDocument>>(json);
            }
            catch (Exception exception)
            {
                throw new FormatException("Could not parse JSON document (which is not included here for privacy reasons)", exception);
            }
        }

        return Parse();
    }

    /// <summary>
    /// Determines whether the given <paramref name="json"/> is valid. This is done by deserializing it to a dynamic JSON object and then verifying that the signature is valid.
    /// </summary>
    public bool IsValid(string json)
    {
        try
        {
            var jObject = JObject.Parse(json);

            var expectedSignature = (jObject[SignatureFieldName] ?? throw new ArgumentException("Could not find 'Signature' element")).Value<string>();
            var document = jObject[DocumentFieldName] ?? throw new ArgumentException("Could not find 'Document' element");

            var jsonToSign = document.ToString(Formatting.None);
            var actualSignature = GenerateSignatureFromString(jsonToSign);

            return string.Equals(actualSignature, expectedSignature);
        }
        catch (Exception exception)
        {
            throw new FormatException("Could not parse JSON document (which is not included here for privacy reasons)", exception);
        }
    }

    /// <summary>
    /// Determines whether the given <paramref name="signed"/> document is valid.
    /// This is done by serializing the wrapped document to JSON object and then verifying that the signature is valid.
    /// </summary>
    public bool IsValid<TDocument>(Signed<TDocument> signed)
    {
        if (signed == null) throw new ArgumentNullException(nameof(signed));
        var actualSignature = GenerateSignatureFromObject(signed.Document);

        return string.Equals(actualSignature, signed.Signature);
    }

    string GenerateSignatureFromObject(object document)
    {
        var jsonToSign = JsonConvert.SerializeObject(document, SerializerSettings);
        return GenerateSignatureFromString(jsonToSign);
    }

    string GenerateSignatureFromString(string json)
    {
        using var hashish = SHA1.Create();

        var jsonBytes = Encoding.UTF8.GetBytes(json);

        using var inputStream = new MemoryStream(jsonBytes);

        var hashBytes = hashish.ComputeHash(inputStream);
        var digest = Encrypt(Convert.ToBase64String(hashBytes));
        var signature = Convert.ToBase64String(digest);

        return signature;
    }

    byte[] Encrypt(string encodedToken)
    {
        var bytes = Encoding.UTF8.GetBytes(encodedToken);

        using var encryptor = _cryptoServiceProvider.CreateEncryptor();
        using var destination = new MemoryStream();
        using var cryptoStream = new CryptoStream(destination, encryptor, CryptoStreamMode.Write);

        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();

        return destination.ToArray();
    }

    /// <summary>
    /// Disposes the wrapped crypto provider
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _cryptoServiceProvider.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }
}