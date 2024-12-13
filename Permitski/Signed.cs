using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
// ReSharper disable RedundantArgumentDefaultValue

namespace Permitski;

/// <summary>
/// Wrapper class for a signed document of type <typeparamref name="TDocument"/>.
/// </summary>
public class Signed<TDocument>(TDocument document, string signature)
{
    /// <summary>
    /// Gets the wrapped document
    /// </summary>
    public TDocument Document { get; } = document ?? throw new ArgumentNullException(nameof(document));

    /// <summary>
    /// Gets the attached signature
    /// </summary>
    public string Signature { get; } = signature ?? throw new ArgumentNullException(nameof(signature));

    /// <summary>
    /// Renders the signed document into its default JSON representation
    /// </summary>
    /// <returns></returns>
    public override string ToString() => ToString(compact: false, singleQuotes: false);

    /// <summary>
    /// Renders the signed document into a JSON representation using the given flags for customization.
    /// </summary>
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