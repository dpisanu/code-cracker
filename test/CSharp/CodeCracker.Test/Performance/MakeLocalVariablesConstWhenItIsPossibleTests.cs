﻿using CodeCracker.Performance;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace CodeCracker.Test.Performance
{
    public class MakeLocalVariablesConstWhenItIsPossibleTests : CodeFixTest<MakeLocalVariableConstWhenItIsPossibleAnalyzer, MakeLocalVariableConstWhenItIsPossibleCodeFixProvider>
    {
        [Fact]
        public async Task IgnoresConstantDeclarations()
        {
            var test = @"const int a = 10;".WrapInMethod();
            await VerifyCSharpHasNoDiagnosticsAsync(test);

        }

        [Fact]
        public async Task IgnoresDeclarationsWithNoInitializers()
        {
            var test = @"int a;".WrapInMethod();
            await VerifyCSharpHasNoDiagnosticsAsync(test);
        }

        [Fact]
        public async Task IgnoresDeclarationsWithNonConstants()
        {
            var test = @"int a = GetValue();".WrapInMethod();
            await VerifyCSharpHasNoDiagnosticsAsync(test);
        }

        [Fact]
        public async Task IgnoresDeclarationsWithReferenceTypes()
        {
            var test = @"Foo a = new Foo();".WrapInMethod();
            await VerifyCSharpHasNoDiagnosticsAsync(test);
        }

        [Fact]
        public async Task IgnoresStringInterpolations()
        {
            var test = @"
            var s = $""a value is {""a""}"";".WrapInMethod();
            await VerifyCSharpHasNoDiagnosticsAsync(test);
        }

        [Fact]
        public async Task IgnoresVariablesThatChangesValueOutsideDeclaration()
        {
            var test = @"int a = 10;a = 20;".WrapInMethod();

            await VerifyCSharpHasNoDiagnosticsAsync(test);
        }

        [Fact]
        public async Task CreateDiagnosticsWhenAssigningAPotentialConstant()
        {
            var test = @"int a = 10;".WrapInMethod();
            var expected = new DiagnosticResult
            {
                Id = MakeLocalVariableConstWhenItIsPossibleAnalyzer.DiagnosticId,
                Message = "This variables can be made const.",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 17) }
            };
            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        [Fact]
        public async Task CreateDiagnosticsWhenAssigningAPotentialConstantInAVarDeclaration()
        {
            var test = @"var a = 10;".WrapInMethod();
            
            var expected = new DiagnosticResult
            {
                Id = MakeLocalVariableConstWhenItIsPossibleAnalyzer.DiagnosticId,
                Message = "This variables can be made const.",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 17) }
            };
            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        [Fact]
        public async Task CreateDiagnosticsWhenAssigningNullToAReferenceType()
        {
            var test = @"Foo a = null;".WrapInMethod();
            
            var expected = new DiagnosticResult
            {
                Id = MakeLocalVariableConstWhenItIsPossibleAnalyzer.DiagnosticId,
                Message = "This variables can be made const.",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 17) }
            };
            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        [Fact]
        public async Task IgnoresNullableVariables()
        {
            var test = "int? a = 1;".WrapInMethod();

            await VerifyCSharpHasNoDiagnosticsAsync(test);
        }

        [Fact]
        public async Task FixMakesAVariableConstWhenDeclarationSpecifiesTypeName()
        {
            var test = @"int a = 10;".WrapInMethod();
            var expected = @"const int a = 10;".WrapInMethod();
            await VerifyCSharpFixAsync(test, expected);
        }

        [Fact]
        public async Task FixMakesAVariableConstWhenDeclarationUsesVar()
        {
            var test = @"var a = 10;".WrapInMethod();
            var expected = @"const int a = 10;".WrapInMethod();
            await VerifyCSharpFixAsync(test, expected);
        }

        [Fact]
        public async Task FixMakesAVariableConstWhenDeclarationUsesVarWithString()
        {
            var test = @"var a = """"".WrapInMethod();
            var expected = @"const string a = """"".WrapInMethod();
            await VerifyCSharpFixAsync(test, expected);
        }

        [Fact]
        public async Task FixMakesAVariableConstWhenSettingNullToAReferenceType()
        {
            const string test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task Foo()
            {
                Fee a = null;
            }
        }

        class Fee {}
    }";

            const string expected = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task Foo()
            {
                const Fee a = null;
            }
        }

        class Fee {}
    }";
            await VerifyCSharpFixAsync(test, expected);
        }


        [Fact]
        public async Task FixMakesAVariableConstWhenUsingVarAsAlias()
        {
            const string test = @"
    using System;
    using var = System.Int32;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task Foo()
            {
                var a = 0;
            }
        }
    }";

            const string expected = @"
    using System;
    using var = System.Int32;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task Foo()
            {
                const var a = 0;
            }
        }
    }";
            await VerifyCSharpFixAsync(test, expected);
        }

        [Fact]
        public async Task FixMakesAVariableConstWhenUsingVarAsClass()
        {
            const string test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task Foo()
            {
                //comment a
                var a = null;
                //comment b
            }
        }

        class var {}
    }";

            const string expected = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task Foo()
            {
                //comment a
                const var a = null;
                //comment b
            }
        }

        class var {}
    }";
            await VerifyCSharpFixAsync(test, expected);
        }

        
    }
}