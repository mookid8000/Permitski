using System.IO;
using System.Text;
using Newtonsoft.Json;
// ReSharper disable RedundantArgumentDefaultValue

namespace Permitski
{
    public class Signed<T>
    {
        public T Document { get; }
        public string Signature { get; }

        public Signed(T document, string signature)
        {
            Document = document;
            Signature = signature;
        }

        public override string ToString() => ToString(compact: false, singleQuotes: false);

        public string ToString(bool compact, bool singleQuotes = true)
        {
            var builder = new StringBuilder();
            var formatting = compact ? Formatting.None : Formatting.Indented;

            using (var output = new StringWriter(builder))
            using (var writer = new JsonTextWriter(output))
            {
                writer.QuoteChar = singleQuotes ? '\'' : '\"';

                var serializer = new JsonSerializer { Formatting = formatting };
                serializer.Serialize(writer, this);
            }

            return builder.ToString();
        }
    }
}