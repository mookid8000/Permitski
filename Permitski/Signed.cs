using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
// ReSharper disable RedundantArgumentDefaultValue

namespace Permitski;

public class Signed<T>
{
    public T Document { get; }
    public string Signature { get; }

    public Signed(T document, string signature)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        Signature = signature ?? throw new ArgumentNullException(nameof(signature));
    }

    public override string ToString() => ToString(compact: false, singleQuotes: false);

    public string ToString(bool compact, bool singleQuotes = true)
    {
        var builder = new StringBuilder();
        var formatting = compact ? Formatting.None : Formatting.Indented;

        using var output = new StringWriter(builder);
        using var writer = new JsonTextWriter(output);

        writer.QuoteChar = singleQuotes ? '\'' : '\"';

        var serializer = new JsonSerializer
        {
            Formatting = formatting, 
            NullValueHandling = NullValueHandling.Ignore
        };
        serializer.Serialize(writer, this);

        return builder.ToString();
    }
}