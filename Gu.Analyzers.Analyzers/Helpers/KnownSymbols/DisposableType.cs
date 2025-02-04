namespace Gu.Analyzers
{
    internal class DisposableType : QualifiedType
    {
        internal readonly QualifiedMethod Dispose;

        internal DisposableType()
            : base("System.IDisposable")
        {
            this.Dispose = new QualifiedMethod(this, "Dispose");
        }
    }
}