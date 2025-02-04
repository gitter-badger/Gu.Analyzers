namespace Gu.Analyzers.Test.GU0031DisposeMemberTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    internal class HappyPath : HappyPathVerifier<GU0031DisposeMember>
    {
        [TestCase("this.stream.Dispose();")]
        [TestCase("this.stream?.Dispose();")]
        [TestCase("stream.Dispose();")]
        [TestCase("stream?.Dispose();")]
        public async Task DisposingField(string disposeCall)
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly Stream stream = File.OpenRead("""");
        
        public void Dispose()
        {
            this.stream.Dispose();
        }
    }";
            testCode = testCode.AssertReplace("this.stream.Dispose();", disposeCall);
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInVirtualDispose()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo : IDisposable
    {
        private readonly Stream stream = File.OpenRead("""");
        private bool disposed;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stream.Dispose();
            }
        }

        protected void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInExpressionBodyDispose()
        {
            var disposableCode = @"
using System;
class Disposable : IDisposable {
    public void Dispose() { }
}";

            var testCode = @"
using System;
class Goof : IDisposable {
    IDisposable _disposable;
    public void Create()  => _disposable = new Disposable();
    public void Dispose() => _disposable.Dispose();
}";
            await this.VerifyHappyPathAsync(disposableCode, testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldAsCast()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly object stream =  File.OpenRead("""");

        public void Dispose()
        {
            var disposable = this.stream as IDisposable;
            disposable?.Dispose();
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInlineAsCast()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly object stream =  File.OpenRead("""");

        public void Dispose()
        {
            (this.stream as IDisposable)?.Dispose();
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldExplicitCast()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly object stream =  File.OpenRead("""");

        public void Dispose()
        {
            var disposable = (IDisposable)this.stream;
            disposable.Dispose();
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInlineExplicitCast()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        private readonly object stream =  File.OpenRead("""");

        public void Dispose()
        {
            ((IDisposable)this.stream).Dispose();
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingPropertyWhenInitializedInProperty()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        public Foo()
        {
            this.Stream = File.OpenRead("""");
        }

        public Stream Stream { get; set; }
        
        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }";

            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingPropertyWhenInitializedInline()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo : IDisposable
    {
        public Stream Stream { get; set; } = File.OpenRead("""");
        
        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }";

            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnorePassedInViaCtor1()
        {
            var testCode = @"
    using System;

    public sealed class Foo
    {
        private readonly IDisposable bar;
        
        public Foo(IDisposable bar)
        {
            this.bar = bar;
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnorePassedInViaCtor2()
        {
            var testCode = @"
    using System;

    public sealed class Foo
    {
        private readonly IDisposable _bar;
        
        public Foo(IDisposable bar)
        {
            _bar = bar;
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnorePassedInViaCtor3()
        {
            var testCode = @"
    using System;

    public sealed class Foo : IDisposable
    {
        private readonly IDisposable _bar;
        
        public Foo(IDisposable bar)
        {
            _bar = bar;
        }

        public void Dispose()
        {
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredWhenNotAssigned()
        {
            var testCode = @"
    using System;
    using System.IO;

    public sealed class Foo
    {
        private readonly IDisposable bar;
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredWhenBackingField()
        {
            var testCode = @"
    using System.IO;

    public sealed class Foo
    {
        private Stream stream;

        public Stream Stream
        {
            get { return this.stream; }
            set { this.stream = value; }
        }
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreFieldThatIsNotDisposable()
        {
            var testCode = @"
    public class Foo
    {
        private readonly object bar = new object();
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreFieldThatIsNotDisposableAssignedWithMethod1()
        {
            var testCode = @"
    public class Foo
    {
        private readonly object bar = Meh();

        private static object Meh() => new object();
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreFieldThatIsNotDisposableAssignedWIthMethod2()
        {
            var testCode = @"
    public class Foo
    {
        private readonly object bar = string.Copy("""");
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredStaticField()
        {
            var testCode = @"
    using System.IO;

    public sealed class Foo
    {
        private static Stream stream = File.OpenRead("""");
    }";
            await this.VerifyHappyPathAsync(testCode)
                      .ConfigureAwait(false);
        }
    }
}