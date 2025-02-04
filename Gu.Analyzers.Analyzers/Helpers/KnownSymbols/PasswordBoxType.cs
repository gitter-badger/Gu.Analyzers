namespace Gu.Analyzers
{
    internal class PasswordBoxType : QualifiedType
    {
        internal readonly QualifiedProperty SecurePassword;

        internal PasswordBoxType()
            : base("System.Windows.Controls.PasswordBox")
        {
            this.SecurePassword = new QualifiedProperty(this, "SecurePassword");
        }
    }
}