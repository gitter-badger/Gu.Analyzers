namespace Gu.Analyzers.Test.GU0032DisposeBeforeReassigningTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class HappyPath : HappyPathVerifier<GU0032DisposeBeforeReassigning>
    {
        [Test]
        public async Task NotDisposingVariable()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        public void Meh()
        {
            var stream = File.OpenRead("""");
            stream.Dispose();
            stream = File.OpenRead("""");
        }
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task NotDisposingField()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private readonly Stream stream = File.OpenRead("""");
        public Foo()
        {
            stream.Dispose();
            stream = File.OpenRead("""");
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}