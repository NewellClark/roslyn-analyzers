// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SealNonPublicTypesAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.SealNonPublicTypesFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SealNonPublicTypesAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.SealNonPublicTypesFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class SealNonPublicTypesTests
    {
        public static IEnumerable<object[]> TrueOptionsTestData
        {
            get
            {
                yield return new[] { string.Empty };
                yield return new[] { $"{OptionsCategoryName}.{EditorConfigOptionNames.ExcludeAssembliesMarkedInternalsVisibleTo} = true" };
                yield return new[] { $"{OptionsCategoryName}.{RuleId}.{EditorConfigOptionNames.ExcludeAssembliesMarkedInternalsVisibleTo} = true" };
            }
        }

        public static IEnumerable<object[]> FalseOptionsTestData
        {
            get
            {
                yield return new[] { $"{OptionsCategoryName}.{EditorConfigOptionNames.ExcludeAssembliesMarkedInternalsVisibleTo} = false" };
                yield return new[] { $"{OptionsCategoryName}.{RuleId}.{EditorConfigOptionNames.ExcludeAssembliesMarkedInternalsVisibleTo} = false" };
            }
        }

        [Theory]
        [MemberData(nameof(TrueOptionsTestData))]
        [MemberData(nameof(FalseOptionsTestData))]
        public Task InternalClass_NoSubclasses_NoFriendAssemblies_Diagnostic_CS(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.CSharp, "friend", "cs");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { @"internal class {|#0:Unsealed|} { }" },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig },
                    ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithArguments("Unsealed").WithLocation(0) }
                },
                FixedState =
                {
                    Sources = { @"internal sealed class Unsealed { }" },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(TrueOptionsTestData))]
        [MemberData(nameof(FalseOptionsTestData))]
        public Task InternalClass_NoSubclasses_NoFriendAssemblies_Diagnostic_VB(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.VisualBasic, "friend", "vb");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Friend Class {|#0:Unsealed|}
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig },
                    ExpectedDiagnostics = { VerifyVB.Diagnostic(Rule).WithArguments("Unsealed").WithLocation(0) },
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
Friend NotInheritable Class Unsealed
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(FalseOptionsTestData))]
        public Task InternalClass_NoSubclasses_WithFriendAssemblies_ExcludeOptionFalse_Diagnostic_CS(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.CSharp, "friend", "cs");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")]
internal class {|#0:Unsealed|} { }"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig },
                    ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithArguments("Unsealed").WithLocation(0) }
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")]
internal sealed class Unsealed { }"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(FalseOptionsTestData))]
        public Task InternalClass_NoSubclasses_WithFriendAssemblies_ExcludeOptionFalse_Diagnostic_VB(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.VisualBasic, "friend", "vb");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
<Assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")>
Friend Class {|#0:Unsealed|}
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig },
                    ExpectedDiagnostics = { VerifyVB.Diagnostic(Rule).WithArguments("Unsealed").WithLocation(0) }
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
<Assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")>
Friend NotInheritable Class Unsealed
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(TrueOptionsTestData))]
        public Task InternalClass_NoSubclasses_WithFriendAssemblies_ExcludeOptionTrue_NoDiagnostic_CS(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.CSharp, "friend", "cs");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")]
internal class Unsealed { }"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(TrueOptionsTestData))]
        public Task InternalClass_NoSubclasses_WithFriendAssemblies_ExcludeOptionTrue_NoDiagnostic_VB(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.VisualBasic, "friend", "vb");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
<Assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")>
Friend Class Unsealed
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(TrueOptionsTestData))]
        [MemberData(nameof(FalseOptionsTestData))]
        public Task PrivateClass_NoSubclasses_Diagnostic_CS(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.CSharp, "friend", "cs");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")]
public sealed class Outer
{
    private class {|#0:Unsealed|} { }
}"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig },
                    ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithArguments("Outer.Unsealed").WithLocation(0) }
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")]
public sealed class Outer
{
    private sealed class Unsealed { }
}"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [MemberData(nameof(TrueOptionsTestData))]
        [MemberData(nameof(FalseOptionsTestData))]
        public Task PrivateClass_NoSubclasses_Diagnostic_VB(string options)
        {
            var friendAssembly = new ProjectState("FriendAssembly", LanguageNames.VisualBasic, "friend", "vb");
            var editorconfig = CreateEditorConfig(options);
            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
<Assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")>
Public NotInheritable Class Outer
    Private Class {|#0:Unsealed|}
    End Class
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig },
                    ExpectedDiagnostics = { VerifyVB.Diagnostic(Rule).WithArguments("FriendAssembly").WithLocation(0) }
                },
                FixedState =
                {
                    Sources =
                    {
                        @"
<Assembly:System.Runtime.CompilerServices.InternalsVisibleTo(""FriendAssembly"")>
Public NotInheritable Class Outer
    Private NotInheritable Class Unsealed
    End Class
End Class"
                    },
                    AdditionalProjects = { { "FriendAssembly", friendAssembly } },
                    AnalyzerConfigFiles = { editorconfig }
                }
            };

            return test.RunAsync();
        }

        #region Helpers
        private static (string FileName, string FileText) CreateEditorConfig(string editorConfigOptions)
        {
            return ("/.editorconfig", $@"root = true
[*]
{editorConfigOptions}");
        }

        [Theory]
        [InlineData("public ")]
        [InlineData("internal ")]
        [InlineData("")]
        public Task AnyTopLevelClass_WithSubclasses_NoDiagnostic_CS(string modifiers)
        {
            return VerifyCS.VerifyAnalyzerAsync(
                $@"
{modifiers}class UnsealedBase {{ }}
{modifiers}sealed class Derived : UnsealedBase {{ }}");
        }

        [Theory]
        [InlineData("Public ")]
        [InlineData("Friend ")]
        [InlineData("")]
        public Task AnyTopLevelClass_WithSubclasses_NoDiagnostic_VB(string modifiers)
        {
            return VerifyVB.VerifyAnalyzerAsync(
                $@"
{modifiers}Class UnsealedBase
End Class
{modifiers}NotInheritable Class Derived : Inherits UnsealedBase
End Class");
        }

        [Theory]
        [InlineData("public ")]
        [InlineData("protected internal ")]
        [InlineData("protected ")]
        [InlineData("internal ")]
        [InlineData("")]
        [InlineData("private protected ")]
        [InlineData("private ")]
        public Task AnyNestedClass_WithSubclasses_NoDiagnostic_CS(string modifiers)
        {
            return VerifyCS.VerifyAnalyzerAsync(
                $@"
public sealed class Outer
{{
    {modifiers}class UnsealedBase {{ }}
    {modifiers}sealed class Derived : UnsealedBase {{ }}
}}");
        }

        [Theory]
        [InlineData("Public ")]
        [InlineData("Protected Friend ")]
        [InlineData("Protected ")]
        [InlineData("Friend ")]
        [InlineData("Private Protected ")]
        [InlineData("private ")]
        public Task AnyNestedClass_WithSubclasses_NoDiagnostic_VB(string modifiers)
        {
            return VerifyVB.VerifyAnalyzerAsync(
                $@"
Public NotInheritable Class Outer
    {modifiers}Class UnsealedBase
    End Class
    {modifiers}NotInheritable Class Derived : Inherits UnsealedBase
    End Class
End Class");
        }

        [Fact]
        public Task StaticClass_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal static class Unsealed { }");
        }

        [Fact]
        public Task StaticClass_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                @"
Friend Module Unsealed
End Module");
        }

        [Fact]
        public Task AbstractClass_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal abstract class Unsealed { }");
        }

        [Fact]
        public Task AbstractClass_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                @"
Friend MustInherit Class Unsealed
End Class");
        }

        [Fact]
        public Task SealedClass_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal sealed class Sealed { }");
        }

        [Fact]
        public Task SealedClass_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                @"
Friend NotInheritable Class Sealed
End Class");
        }

        [Fact]
        public Task Struct_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal struct MyStruct { }");
        }

        [Fact]
        public Task Struct_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                @"
Friend Structure MyStruct
End Structure");
        }

        [Fact]
        public Task Interface_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal interface IUnsealed { }");
        }

        [Fact]
        public Task Interface_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                @"
Friend Interface IUnsealed
End Interface");
        }

        [Fact]
        public Task Delegate_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal delegate void MyAction();");
        }

        [Fact]
        public Task Delegate_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(@"Friend Delegate Sub MyAction()");
        }

        [Fact]
        public Task Enum_NoDiagnostic_CS()
        {
            return VerifyCS.VerifyAnalyzerAsync(@"internal enum MyEnum { None }");
        }

        [Fact]
        public Task Enum_NoDiagnostic_VB()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                @"
Friend Enum MyEnum
    None
End Enum");
        }

        private static DiagnosticDescriptor Rule => SealNonPublicTypesAnalyzer.Rule;

        private static string RuleId => SealNonPublicTypesAnalyzer.RuleId;

        private const string OptionsCategoryName = "dotnet_code_quality";
        #endregion
    }
}
