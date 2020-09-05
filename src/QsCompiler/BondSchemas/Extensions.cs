// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;

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
            return default;
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
