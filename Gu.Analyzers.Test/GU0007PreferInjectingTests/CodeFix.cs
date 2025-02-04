﻿namespace Gu.Analyzers.Test.GU0007PreferInjectingTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class CodeFix : CodeFixVerifier<GU0007PreferInjecting, InjectCodeFixProvider>
    {
        [Test]
        public async Task WhenNotInjectingFieldInitialization()
        {
            var fooCode = @"
    public class Foo
    {
        private readonly Bar bar;

        public Foo()
        {
            this.bar = ↓new Bar();
        }
    }";
            var barCode = @"
    public class Bar
    {
    }";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref fooCode)
                               .WithMessage("Prefer injecting.");
            await this.VerifyCSharpDiagnosticAsync(new[] { fooCode, barCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
    public class Foo
    {
        private readonly Bar bar;

        public Foo(Bar bar)
        {
            this.bar = bar;
        }
    }";
            await this.VerifyCSharpFixAsync(new[] { fooCode, barCode }, new[] { fixedCode, barCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotInjectingChained()
        {
            var fooCode = @"
    public class Foo
    {
        private readonly Bar bar;

        public Foo(Bar bar)
        {
            this.bar = bar;
        }
    }";
            var barCode = @"
    public class Bar
    {
    }";

            var mehCode = @"
    public class Meh : Foo
    {
        public Meh()
           : base(↓new Bar())
        {
        }
    }";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref mehCode)
                               .WithMessage("Prefer injecting.");
            await this.VerifyCSharpDiagnosticAsync(new[] { fooCode, barCode, mehCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
    public class Meh : Foo
    {
        public Meh(Bar bar)
           : base(bar)
        {
        }
    }";
            await this.VerifyCSharpFixAsync(new[] { fooCode, barCode, mehCode }, new[] { fooCode, barCode, fixedCode }).ConfigureAwait(false);
        }
    }
}
