namespace Gu.Analyzers.Test.GU0004AssignAllReadOnlyMembersTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class Diagnostics : DiagnosticVerifier<GU0004AssignAllReadOnlyMembers>
    {
        [Test]
        public async Task NotSettingGetOnlyProperty()
        {
            var testCode = @"
    public class Foo
    {
        ↓public Foo(int a)
        {
            this.A = a;
        }

        public int A { get; }

        public int B { get; }
    }";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Assign all readonly members.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task NotSettingReadOnlyField()
        {
            var testCode = @"
    public class Foo
    {
        private readonly int a;
        private readonly int b;

        ↓public Foo(int a)
        {
            this.a = a;
        }
    }";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Assign all readonly members.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }
    }
}