namespace Gu.Analyzers
{
    internal class StringType : QualifiedType
    {
        internal readonly QualifiedMethod Format;

        internal StringType()
            : base("System.String")
        {
            this.Format = new QualifiedMethod(this, "Format");
        }
    }
}