using Newtonsoft.Json;

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

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}