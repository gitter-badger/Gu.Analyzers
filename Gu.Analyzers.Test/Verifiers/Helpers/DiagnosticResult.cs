﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// ReSharper disable ParameterHidesMember
namespace Gu.Analyzers.Test
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Structure that stores information about a <see cref="Diagnostic"/> appearing in a source.
    /// </summary>
    public struct DiagnosticResult
    {
        private const string DefaultPath = "Test0.cs";

        private static readonly object[] EmptyArguments = new object[0];

        private FileLinePositionSpan[] spans;
        private string message;

        public DiagnosticResult(DiagnosticDescriptor descriptor)
            : this()
        {
            this.Id = descriptor.Id;
            this.Severity = descriptor.DefaultSeverity;
            this.MessageFormat = descriptor.MessageFormat;
        }

        public FileLinePositionSpan[] Spans
        {
            get
            {
                return this.spans ?? (this.spans = new FileLinePositionSpan[] { });
            }

            set
            {
                this.spans = value;
            }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message
        {
            get
            {
                if (this.message != null)
                {
                    return this.message;
                }

                if (this.MessageFormat != null)
                {
                    return string.Format(this.MessageFormat.ToString(), this.MessageArguments ?? EmptyArguments);
                }

                return null;
            }

            set
            {
                this.message = value;
            }
        }

        public LocalizableString MessageFormat { get; set; }

        public object[] MessageArguments { get; set; }

        public bool HasLocation => (this.spans != null) && (this.spans.Length > 0);

        public DiagnosticResult WithArguments(params object[] arguments)
        {
            var result = this;
            result.MessageArguments = arguments;
            return result;
        }

        public DiagnosticResult WithMessage(string message)
        {
            var result = this;
            result.Message = message;
            return result;
        }

        public DiagnosticResult WithMessageFormat(LocalizableString messageFormat)
        {
            var result = this;
            result.MessageFormat = messageFormat;
            return result;
        }

        public DiagnosticResult WithLocationIndicated(ref string testCode)
        {
            var pos = DiagnosticVerifier.GetErrorPosition(ref testCode);
            return this.AppendSpan(pos);
        }

        public DiagnosticResult WithLocationIndicated(string[] testCode)
        {
            var pos = DiagnosticVerifier.GetErrorPosition(testCode);
            return this.AppendSpan(pos);
        }

        public DiagnosticResult WithLocation(int line, int column)
        {
            return this.WithLocation(DefaultPath, line, column);
        }

        public DiagnosticResult WithLocation(string path, int line, int column)
        {
            var linePosition = new LinePosition(line, column);
            return this.AppendSpan(new FileLinePositionSpan(path, linePosition, linePosition));
        }

        public DiagnosticResult WithSpan(int startLine, int startColumn, int endLine, int endColumn)
        {
            return this.WithSpan(DefaultPath, startLine, startColumn, endLine, endColumn);
        }

        public DiagnosticResult WithSpan(string path, int startLine, int startColumn, int endLine, int endColumn)
        {
            return this.AppendSpan(new FileLinePositionSpan(path, new LinePosition(startLine, startColumn), new LinePosition(endLine, endColumn)));
        }

        public DiagnosticResult WithLineOffset(int offset)
        {
            var result = this;
            Array.Resize(ref result.spans, result.spans?.Length ?? 0);
            for (var i = 0; i < result.spans.Length; i++)
            {
                var newStartLinePosition = new LinePosition(result.spans[i].StartLinePosition.Line + offset, result.spans[i].StartLinePosition.Character);
                var newEndLinePosition = new LinePosition(result.spans[i].EndLinePosition.Line + offset, result.spans[i].EndLinePosition.Character);

                result.spans[i] = new FileLinePositionSpan(result.spans[i].Path, newStartLinePosition, newEndLinePosition);
            }

            return result;
        }

        private DiagnosticResult AppendSpan(FileLinePositionSpan span)
        {
            FileLinePositionSpan[] newSpans;

            if (this.spans != null)
            {
                newSpans = new FileLinePositionSpan[this.spans.Length + 1];
                Array.Copy(this.spans, newSpans, this.spans.Length);
                newSpans[this.spans.Length] = span;
            }
            else
            {
                newSpans = new[] { span };
            }

            // clone the object, so that the fluent syntax will work on immutable objects.
            return new DiagnosticResult
            {
                Id = this.Id,
                Message = this.message,
                MessageFormat = this.MessageFormat,
                MessageArguments = this.MessageArguments,
                Severity = this.Severity,
                Spans = newSpans,
            };
        }
    }
}
