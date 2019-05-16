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

        public override string ToString() => ToString(compact: false);

        public string ToString(bool compact) => JsonConvert.SerializeObject(this, compact ? Formatting.None : Formatting.Indented);
    }
}