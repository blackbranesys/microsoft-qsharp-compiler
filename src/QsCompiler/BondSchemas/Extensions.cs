// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Quantum.QsCompiler.DataTypes;

namespace Microsoft.Quantum.QsCompiler.BondSchemas
{
    public static class Extensions
    {
        public static QsCompilation CreateBondCompilation(SyntaxTree.QsCompilation qsCompilation)
        {
            var bondQscompilation = new QsCompilation { };
            foreach (var qsNamespace in qsCompilation.Namespaces)
            {
                bondQscompilation.Namespaces.Add(qsNamespace.ToBondSchema());
            }

            foreach (var entryPoint in qsCompilation.EntryPoints)
            {
                bondQscompilation.EntryPoints.Add(entryPoint.ToBondSchema());
            }

            return bondQscompilation;
        }

        public static SyntaxTree.QsCompilation CreateQsCompilation(QsCompilation bondCompilation)
        {
            var namespaces = new SyntaxTree.QsNamespace[bondCompilation.Namespaces.Count];
            var index = 0;
            foreach(var bondNamespace in bondCompilation.Namespaces)
            {
                var elements = Array.Empty<SyntaxTree.QsNamespaceElement>();
                namespaces[index] = new SyntaxTree.QsNamespace(
                    NonNullable<string>.New(bondNamespace.Name),
                    elements.ToImmutableArray(),
                    bondNamespace.Documentation.ToLookup(
                        p => NonNullable<string>.New(p.SourceFileName),
                        p => p.DocumentationInstances.ToImmutableArray()));

                index++;
            }

            var entryPoints = Array.Empty<SyntaxTree.QsQualifiedName>();
            return new SyntaxTree.QsCompilation(namespaces.ToImmutableArray(), entryPoints.ToImmutableArray());
        }

        private static QsCallable ToBondSchema(this SyntaxTree.QsCallable qsCallable)
        {
            var bondQsCallable = new QsCallable
            {
                // TODO: Populate.
            };

            return bondQsCallable;
        }

        private static QsCustomType ToBondSchema(this SyntaxTree.QsCustomType qsCustomType)
        {
            var bondQsCustomType = new QsCustomType
            {
                // TODO: Populate.
            };

            return bondQsCustomType;
        }

        private static QsQualifiedName ToBondSchema(this SyntaxTree.QsQualifiedName qsQualifiedName)
        {
            var bondQsQualifiedName = new QsQualifiedName
            {
                Namespace = qsQualifiedName.Namespace.Value,
                Name = qsQualifiedName.Name.Value
            };

            return bondQsQualifiedName;
        }

        private static QsNamespace ToBondSchema(this SyntaxTree.QsNamespace qsNamespace)
        {
            var bondQsNamespace = new QsNamespace
            {
                Name = qsNamespace.Name.Value
            };

            foreach (var qsNamespaceElement in qsNamespace.Elements)
            {
                bondQsNamespace.Elements.Add(qsNamespaceElement.ToBondSchema());
            }

            foreach (var sourceFileDocumentation in qsNamespace.Documentation)
            {
                foreach(var item in sourceFileDocumentation)
                {
                    var qsDocumentationItem = new QsDocumentationItem
                    {
                        SourceFileName = sourceFileDocumentation.Key.Value,
                        DocumentationInstances = item.ToList()
                    };

                    bondQsNamespace.Documentation.AddLast(qsDocumentationItem);
                }

            }

            return bondQsNamespace;
        }

        private static QsNamespaceElement ToBondSchema(this SyntaxTree.QsNamespaceElement qsNamespaceElement)
        {
            QsNamespaceElementKind kind;
            SyntaxTree.QsCallable qsCallable = null;
            SyntaxTree.QsCustomType qsCustomType = null;
            if (qsNamespaceElement.TryGetCallable(ref qsCallable))
            {
                kind = QsNamespaceElementKind.QsCallable;
            }
            else if (qsNamespaceElement.TryGetCustomType(ref qsCustomType))
            {
                kind = QsNamespaceElementKind.QsCustomType;
            }
            else
            {
                throw new ArgumentException($"Unsupported {typeof(SyntaxTree.QsNamespaceElement)} kind");
            }

            var bondQsNamespaceElement = new QsNamespaceElement
            {
                Kind = kind,
                Callable = qsCallable?.ToBondSchema(),
                CustomType = qsCustomType?.ToBondSchema()
            };

            return bondQsNamespaceElement;
        }
    }
}
