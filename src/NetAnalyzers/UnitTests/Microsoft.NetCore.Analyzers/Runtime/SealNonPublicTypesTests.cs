// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
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

    }
}
