// ReSharper disable InconsistentNaming
namespace Gu.Analyzers
{
    internal static class KnownSymbol
    {
        ////internal static readonly QualifiedType Object = Create("System.Object");
        internal static readonly StringType String = new StringType();
        internal static readonly QualifiedType Tuple = Create("System.Tuple");
        internal static readonly DisposableType IDisposable = new DisposableType();
        internal static readonly QualifiedType ArgumentException = Create("System.ArgumentException");
        internal static readonly QualifiedType ArgumentNullException = Create("System.ArgumentNullException");
        internal static readonly QualifiedType ArgumentOutOfRangeException = Create("System.ArgumentOutOfRangeException");

        internal static readonly QualifiedType Expression = Create("System.Linq.Expressions.Expression");
        internal static readonly DependencyPropertyType DependencyProperty = new DependencyPropertyType();

        internal static readonly PasswordBoxType PasswordBox = new PasswordBoxType();

        private static QualifiedType Create(string qualifiedName)
        {
            return new QualifiedType(qualifiedName);
        }
    }
}