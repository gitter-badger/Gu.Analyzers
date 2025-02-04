﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 169
namespace Gu.Analyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Text;
    using NUnit.Framework;

    /// <summary>
    /// Class for turning strings into documents and getting the diagnostics on them.
    /// All methods are static.
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        private static readonly string DefaultFilePathPrefix = "Test";
        private static readonly string CSharpDefaultFileExt = "cs";
        private static readonly string VisualBasicDefaultExt = "vb";
        private static readonly string TestProjectName = "TestProject";

        internal static string[] CreateFileNamesFromSources(string[] sources, string extension)
        {
            var filenames = new string[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                string name;
                if (source == string.Empty)
                {
                    name = "Test";
                }
                else
                {
                    var matches = Regex.Matches(source, @"(class|struct|enum|interface) (?<name>\w+)(<(?<typeArg>\w+)>)?", RegexOptions.ExplicitCapture);
                    if (matches.Count == 0)
                    {
                        name = "AssemblyInfo";
                    }
                    else
                    {
                        Assert.LessOrEqual(1, matches.Count, "Use class per file, it catches more bugs");
                        name = matches[0].Groups["name"].Value;
                        if (matches[0].Groups["typeArg"].Success)
                        {
                            name += $"{{{matches[0].Groups["typeArg"].Value}}}";
                        }
                    }
                }

                var suffixCount = 0;
                while (true)
                {
                    var fileName = $"{name}{new string('_', suffixCount)}.{extension}";
                    if (filenames.Contains(fileName))
                    {
                        suffixCount++;
                        continue;
                    }

                    filenames[i] = fileName;
                    break;
                }
            }

            return filenames;
        }

        /// <summary>
        /// Given an analyzer and a collection of documents to apply it to, run the analyzer and gather an array of
        /// diagnostics found. The returned diagnostics are then ordered by location in the source documents.
        /// </summary>
        /// <param name="analyzers">The analyzer to run on the documents.</param>
        /// <param name="documents">The <see cref="Document"/>s that the analyzer will be run on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A collection of <see cref="Diagnostic"/>s that surfaced in the source code, sorted by
        /// <see cref="Diagnostic.Location"/>.</returns>
        protected static async Task<ImmutableArray<Diagnostic>> GetSortedDiagnosticsFromDocumentsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, Document[] documents, CancellationToken cancellationToken)
        {
            var projects = new HashSet<Project>();
            foreach (var document in documents)
            {
                projects.Add(document.Project);
            }

            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (var project in projects)
            {
                var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
                var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, project.AnalyzerOptions, cancellationToken);
                var compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);
                var compilerErrors = compilerDiagnostics.Where(i => i.Severity == DiagnosticSeverity.Error);
                var diags = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
                var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync().ConfigureAwait(false);
                var failureDiagnostics = allDiagnostics.Where(diagnostic => diagnostic.Id == "AD0001");
                foreach (var diag in diags.Concat(compilerErrors).Concat(failureDiagnostics))
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else
                    {
                        for (var i = 0; i < documents.Length; i++)
                        {
                            var document = documents[i];
                            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
                            if (tree == diag.Location.SourceTree)
                            {
                                diagnostics.Add(diag);
                            }
                        }
                    }
                }
            }

            var results = SortDistinctDiagnostics(diagnostics);
            return results.ToImmutableArray();
        }

        /// <summary>
        /// Create a <see cref="Document"/> from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string.</param>
        /// <param name="language">The language the source classes are in. Values may be taken from the
        /// <see cref="LanguageNames"/> class.</param>
        /// <param name="fileName">The file name for the document, or <see langword="null"/> to generate a default
        /// filename according to the specified <paramref name="language"/>.</param>
        /// <returns>A <see cref="Document"/> created from the source string.</returns>
        protected Document CreateDocument(string source, string language = LanguageNames.CSharp, string fileName = null)
        {
            string[] filenames = null;
            if (fileName != null)
            {
                filenames = new[] { fileName };
            }

            return this.CreateProject(new[] { source }, language, filenames).Documents.Single();
        }

        /// <summary>
        /// Creates a solution that will be used as parent for the sources that need to be checked.
        /// </summary>
        /// <param name="projectId">The project identifier to use.</param>
        /// <param name="language">The language for which the solution is being created.</param>
        /// <returns>The created solution.</returns>
        protected virtual Solution CreateSolution(ProjectId projectId, string language)
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, language)
                .WithProjectCompilationOptions(projectId, compilationOptions)
                .AddMetadataReference(projectId, MetadataReferences.CorlibReference)
                .AddMetadataReference(projectId, MetadataReferences.SystemReference)
                .AddMetadataReference(projectId, MetadataReferences.SystemCoreReference)
                .AddMetadataReference(projectId, MetadataReferences.PresentationCoreReference)
                .AddMetadataReference(projectId, MetadataReferences.PresentationFrameworkReference)
                .AddMetadataReference(projectId, MetadataReferences.WindowsBaseReference)
                .AddMetadataReference(projectId, MetadataReferences.SystemXamlReference)
                .AddMetadataReference(projectId, MetadataReferences.CSharpSymbolsReference)
                .AddMetadataReference(projectId, MetadataReferences.CodeAnalysisReference);

            solution.Workspace.Options =
                solution.Workspace.Options
                .WithChangedOption(FormattingOptions.IndentationSize, language, this.IndentationSize)
                .WithChangedOption(FormattingOptions.TabSize, language, this.TabSize)
                .WithChangedOption(FormattingOptions.UseTabs, language, this.UseTabs);

            var parseOptions = solution.GetProject(projectId).ParseOptions;
            return solution.WithProjectParseOptions(projectId, parseOptions.WithDocumentationMode(DocumentationMode.Diagnose));
        }

        /// <summary>
        /// Gets the diagnostics that will be suppressed.
        /// </summary>
        /// <returns>A collection of diagnostic identifiers.</returns>
        protected virtual IEnumerable<string> GetDisabledDiagnostics()
        {
            return Enumerable.Empty<string>();
        }

        protected DiagnosticResult CSharpDiagnostic(string diagnosticId = null)
        {
            var analyzers = this.GetCSharpDiagnosticAnalyzers();
            var supportedDiagnostics = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics);
            if (diagnosticId == null)
            {
                return this.CSharpDiagnostic(supportedDiagnostics.Single());
            }
            else
            {
                return this.CSharpDiagnostic(supportedDiagnostics.Single(i => i.Id == diagnosticId));
            }
        }

        protected DiagnosticResult CSharpDiagnostic(DiagnosticDescriptor descriptor)
        {
            return new DiagnosticResult(descriptor);
        }

        protected DiagnosticResult CSharpCompilerError(string errorIdentifier)
        {
            return new DiagnosticResult
            {
                Id = errorIdentifier,
                Severity = DiagnosticSeverity.Error,
            };
        }

        /// <summary>
        /// Create a project using the input strings as sources.
        /// </summary>
        /// <remarks>
        /// <para>This method first creates a <see cref="Project"/> by calling <see cref="CreateProjectImpl"/>, and then
        /// applies compilation options to the project by calling <see cref="ApplyCompilationOptions"/>.</para>
        /// </remarks>
        /// <param name="sources">Classes in the form of strings.</param>
        /// <param name="language">The language the source classes are in. Values may be taken from the
        /// <see cref="LanguageNames"/> class.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Project"/> created out of the <see cref="Document"/>s created from the source
        /// strings.</returns>
        protected Project CreateProject(string[] sources, string language = LanguageNames.CSharp, string[] filenames = null)
        {
            var project = this.CreateProjectImpl(sources, language, filenames);
            return this.ApplyCompilationOptions(project);
        }

        /// <summary>
        /// Create a project using the input strings as sources.
        /// </summary>
        /// <param name="sources">Classes in the form of strings.</param>
        /// <param name="language">The language the source classes are in. Values may be taken from the
        /// <see cref="LanguageNames"/> class.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Project"/> created out of the <see cref="Document"/>s created from the source
        /// strings.</returns>
        protected virtual Project CreateProjectImpl(string[] sources, string language, string[] filenames)
        {
            var fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);
            var solution = this.CreateSolution(projectId, language);

            if (filenames == null)
            {
                filenames = CreateFileNamesFromSources(sources, fileExt);
            }

            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                var newFileName = filenames[i];
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
            }

            return solution.GetProject(projectId);
        }

        /// <summary>
        /// Applies compilation options to a project.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation configures the project by enabling all supported diagnostics of analyzers
        /// included in <see cref="GetCSharpDiagnosticAnalyzers"/> as well as <c>AD0001</c>. After configuring these
        /// diagnostics, any diagnostic IDs indicated in <see cref="GetDisabledDiagnostics"/> are explictly supressed
        /// using <see cref="ReportDiagnostic.Suppress"/>.</para>
        /// </remarks>
        /// <param name="project">The project.</param>
        /// <returns>The modified project.</returns>
        protected virtual Project ApplyCompilationOptions(Project project)
        {
            var analyzers = this.GetCSharpDiagnosticAnalyzers();

            var supportedDiagnosticsSpecificOptions = new Dictionary<string, ReportDiagnostic>();
            foreach (var analyzer in analyzers)
            {
                foreach (var diagnostic in analyzer.SupportedDiagnostics)
                {
                    // make sure the analyzers we are testing are enabled
                    supportedDiagnosticsSpecificOptions[diagnostic.Id] = ReportDiagnostic.Default;
                }
            }

            // Report exceptions during the analysis process as errors
            supportedDiagnosticsSpecificOptions.Add("AD0001", ReportDiagnostic.Error);

            foreach (var id in this.GetDisabledDiagnostics())
            {
                supportedDiagnosticsSpecificOptions[id] = ReportDiagnostic.Suppress;
            }

            // update the project compilation options
            var modifiedSpecificDiagnosticOptions = supportedDiagnosticsSpecificOptions.ToImmutableDictionary().SetItems(project.CompilationOptions.SpecificDiagnosticOptions);
            var modifiedCompilationOptions = project.CompilationOptions.WithSpecificDiagnosticOptions(modifiedSpecificDiagnosticOptions);

            var solution = project.Solution.WithProjectCompilationOptions(project.Id, modifiedCompilationOptions);
            return solution.GetProject(project.Id);
        }

        /// <summary>
        /// Sort <see cref="Diagnostic"/>s by location in source document.
        /// </summary>
        /// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s to be sorted.</param>
        /// <returns>A collection containing the input <paramref name="diagnostics"/>, sorted by
        /// <see cref="Diagnostic.Location"/> and <see cref="Diagnostic.Id"/>.</returns>
        private static Diagnostic[] SortDistinctDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ThenBy(d => d.Id).ToArray();
        }

        /// <summary>
        /// Given classes in the form of strings, their language, and an <see cref="DiagnosticAnalyzer"/> to apply to
        /// it, return the <see cref="Diagnostic"/>s found in the string after converting it to a
        /// <see cref="Document"/>.
        /// </summary>
        /// <param name="sources">Classes in the form of strings.</param>
        /// <param name="language">The language the source classes are in. Values may be taken from the
        /// <see cref="LanguageNames"/> class.</param>
        /// <param name="analyzers">The analyzers to be run on the sources.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A collection of <see cref="Diagnostic"/>s that surfaced in the source code, sorted by
        /// <see cref="Diagnostic.Location"/>.</returns>
        private Task<ImmutableArray<Diagnostic>> GetSortedDiagnosticsAsync(string[] sources, string language, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken, string[] filenames)
        {
            return GetSortedDiagnosticsFromDocumentsAsync(analyzers, this.GetDocuments(sources, language, filenames), cancellationToken);
        }

        /// <summary>
        /// Given an array of strings as sources and a language, turn them into a <see cref="Project"/> and return the
        /// documents and spans of it.
        /// </summary>
        /// <param name="sources">Classes in the form of strings.</param>
        /// <param name="language">The language the source classes are in. Values may be taken from the
        /// <see cref="LanguageNames"/> class.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A collection of <see cref="Document"/>s representing the sources.</returns>
        private Document[] GetDocuments(string[] sources, string language, string[] filenames)
        {
            if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
            {
                throw new ArgumentException("Unsupported Language");
            }

            var project = this.CreateProject(sources, language, filenames);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new SystemException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }
    }
}
