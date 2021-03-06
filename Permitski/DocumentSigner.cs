﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Permitski.Internals;

namespace Permitski
{
    public class DocumentSigner : IDisposable
    {
        public static string GenerateKey() => Crypto.GenerateNewKey();

        readonly AesCryptoServiceProvider _cryptoServiceProvider;

        bool _disposed;

        public DocumentSigner(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _cryptoServiceProvider = Crypto.GetFromKey(key);
        }

        public Signed<TDocument> Sign<TDocument>(TDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            return new Signed<TDocument>(document, GenerateSignatureFromObject(document));
        }

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

        public bool IsValid(string json)
        {
            try
            {
                var jObject = JObject.Parse(json);

                var expectedSignature = (jObject[nameof(Signed<object>.Signature)]
                                         ?? throw new ArgumentException("Could not find 'Signature' element"))
                    .Value<string>();

                var document = jObject[nameof(Signed<object>.Document)]
                               ?? throw new ArgumentException("Could not find 'Document' element");

                var jsonToSign = document.ToString(Formatting.None);
                var actualSignature = GenerateSignatureFromString(jsonToSign);

                return string.Equals(actualSignature, expectedSignature);
            }
            catch (Exception exception)
            {
                throw new FormatException("Could not parse JSON document (which is not included here for privacy reasons)", exception);
            }
        }

        public bool IsValid<TDocument>(Signed<TDocument> signed)
        {
            if (signed == null) throw new ArgumentNullException(nameof(signed));
            var actualSignature = GenerateSignatureFromObject(signed.Document);

            return string.Equals(actualSignature, signed.Signature);
        }

        string GenerateSignatureFromObject(object document)
        {
            var jsonToSign = JsonConvert.SerializeObject(document);
            return GenerateSignatureFromString(jsonToSign);
        }

        string GenerateSignatureFromString(string json)
        {
            using (var hashish = SHA1.Create())
            {
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                using (var inputStream = new MemoryStream(jsonBytes))
                {
                    var hashBytes = hashish.ComputeHash(inputStream);
                    var digest = Encrypt(Convert.ToBase64String(hashBytes));
                    var signature = Convert.ToBase64String(digest);
                    return signature;
                }
            }
        }

        byte[] Encrypt(string encodedToken)
        {
            var bytes = Encoding.UTF8.GetBytes(encodedToken);

            using (var encryptor = _cryptoServiceProvider.CreateEncryptor())
            using (var destination = new MemoryStream())
            using (var cryptoStream = new CryptoStream(destination, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();
                return destination.ToArray();
            }
        }

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
}
