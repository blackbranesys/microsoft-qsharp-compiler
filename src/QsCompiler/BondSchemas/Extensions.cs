// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            var namespaces = new List<SyntaxTree.QsNamespace>();
            foreach(var bondNamespace in bondCompilation.Namespaces)
            {
                namespaces.Add(bondNamespace.ToSyntaxTreeObject());
            }

            var entryPoints = Array.Empty<SyntaxTree.QsQualifiedName>();
            return new SyntaxTree.QsCompilation(namespaces.ToImmutableArray(), entryPoints.ToImmutableArray());
        }

        private static QsCallable ToBondSchema(this SyntaxTree.QsCallable qsCallable)
        {
            var bondQsCallable = new QsCallable
            {
                Kind = qsCallable.Kind.ToBondSchema(),
                FullName = qsCallable.FullName.ToBondSchema(),
                // TODO: Populate.
                Documentation = qsCallable.Documentation.ToList(),
                Comments = qsCallable.Comments.ToBondSchema()
            };

            return bondQsCallable;
        }

        private static QsCallableKind ToBondSchema(this SyntaxTree.QsCallableKind qsCallableKind)
        {
            if (qsCallableKind.IsOperation)
            {
                return QsCallableKind.Operation;
            }
            else if (qsCallableKind.IsFunction)
            {
                return QsCallableKind.Function;
            }
            else if (qsCallableKind.IsTypeConstructor)
            {
                return QsCallableKind.TypeConstructor;
            }

            throw new ArgumentException($"Unsupported QsCallableKind {qsCallableKind}");
        }

        private static QsComments ToBondSchema(this SyntaxTree.QsComments qsComments)
        {
            var bondQsComments = new QsComments
            {
                OpeningComments = qsComments.OpeningComments.ToList(),
                ClosingComments = qsComments.ClosingComments.ToList()
            };

            return bondQsComments;
        }

        private static QsCustomType ToBondSchema(this SyntaxTree.QsCustomType qsCustomType)
        {
            var bondQsCustomType = new QsCustomType
            {
                FullName = qsCustomType.FullName.ToBondSchema(),
                Documentation = qsCustomType.Documentation.ToList(),
                Comments = qsCustomType.Comments.ToBondSchema()
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

        private static DataTypes.Position ToDataTypeObject(this Position position) =>
            DataTypes.Position.Create(position.Line, position.Column);

        private static DataTypes.Range ToDataTypeObject(this Range range) =>
            DataTypes.Range.Create(range.Start.ToDataTypeObject(), range.End.ToDataTypeObject());

        private static NonNullable<string> ToNonNullable(this string str) =>
            NonNullable<string>.New(str);

        private static QsNullable<SyntaxTree.QsLocation> ToQsNullable(this SyntaxTree.QsLocation qsLocation) =>
            QsNullable<SyntaxTree.QsLocation>.NewValue(qsLocation);

        private static SyntaxTree.QsCallable ToSyntaxTreeObject(this QsCallable bondQsCallable) =>
            new SyntaxTree.QsCallable(
                kind: bondQsCallable.Kind.ToSyntaxtTreeObject(),
                fullName: bondQsCallable.FullName.ToSyntaxTreeObject(),
                // TODO: Implement.
                attributes: Array.Empty<SyntaxTree.QsDeclarationAttribute>().ToImmutableArray(),
                // TODO: Get this from the bond object.
                modifiers: new SyntaxTokens.Modifiers(),
                sourceFile: bondQsCallable.SourceFile.ToNonNullable(),
                location: bondQsCallable.Location.ToSyntaxTreeObject().ToQsNullable(),
                // TODO: Implement.
                signature: default,
                argumentTuple: default,
                specializations: Array.Empty<SyntaxTree.QsSpecialization>().ToImmutableArray(),
                documentation: bondQsCallable.Documentation.ToImmutableArray(),
                comments: bondQsCallable.Comments.ToSyntaxTreeObject());

        private static SyntaxTree.QsCallableKind ToSyntaxtTreeObject(this QsCallableKind bondQsCallableKind) =>
            bondQsCallableKind switch
            {
                QsCallableKind.Operation => SyntaxTree.QsCallableKind.Operation,
                QsCallableKind.Function => SyntaxTree.QsCallableKind.Function,
                QsCallableKind.TypeConstructor => SyntaxTree.QsCallableKind.TypeConstructor,
                _ => throw new ArgumentException($"Unsupported Bond QsCallableKind: {bondQsCallableKind}")
            };

        private static SyntaxTree.QsComments ToSyntaxTreeObject(this QsComments bondQsComments) =>
            new SyntaxTree.QsComments(
                bondQsComments.OpeningComments.ToImmutableArray(),
                bondQsComments.ClosingComments.ToImmutableArray());

        private static SyntaxTree.QsCustomType ToSyntaxTreeObject(this QsCustomType bondQsCustomType) =>
            new SyntaxTree.QsCustomType(
                fullName: bondQsCustomType.FullName.ToSyntaxTreeObject(),
                // TODO: Implement needed extensions.
                attributes: Array.Empty<SyntaxTree.QsDeclarationAttribute>().ToImmutableArray(),
                // TODO: Get this from the bond object.
                modifiers: new SyntaxTokens.Modifiers(),
                sourceFile: bondQsCustomType.SourceFile.ToNonNullable(),
                location: bondQsCustomType.Location.ToSyntaxTreeObject().ToQsNullable(),
                // TODO: Implement this.
                type: default,
                // TODO: Implement this.
                typeItems: default,
                documentation: bondQsCustomType.Documentation.ToImmutableArray(),
                comments: bondQsCustomType.Comments.ToSyntaxTreeObject());

        private static SyntaxTree.QsLocation ToSyntaxTreeObject(this QsLocation bondQsLocation) =>
            bondQsLocation != null ?
                new SyntaxTree.QsLocation(bondQsLocation.Offset.ToDataTypeObject(), bondQsLocation.Range.ToDataTypeObject()) :
                null;

        private static SyntaxTree.QsNamespace ToSyntaxTreeObject(this QsNamespace bondQsNamespace)
        {
            var elements = new List<SyntaxTree.QsNamespaceElement>();
            foreach(var bondNamespaceElement in bondQsNamespace.Elements)
            {
                elements.Add(bondNamespaceElement.ToSyntaxTreeObject());
            }

            return new SyntaxTree.QsNamespace(
                bondQsNamespace.Name.ToNonNullable(),
                elements.ToImmutableArray(),
                bondQsNamespace.Documentation.ToLookup(
                    p => p.SourceFileName.ToNonNullable(),
                    p => p.DocumentationInstances.ToImmutableArray()));
        }

        private static SyntaxTree.QsNamespaceElement ToSyntaxTreeObject(this QsNamespaceElement bondQsNamespaceElement)
        {
            if (bondQsNamespaceElement.Kind == QsNamespaceElementKind.QsCallable)
            {
                return SyntaxTree.QsNamespaceElement.NewQsCallable(bondQsNamespaceElement.Callable.ToSyntaxTreeObject());
            }
            else if (bondQsNamespaceElement.Kind == QsNamespaceElementKind.QsCustomType)
            {
                return SyntaxTree.QsNamespaceElement.NewQsCustomType(bondQsNamespaceElement.CustomType.ToSyntaxTreeObject());
            }
            else
            {
                throw new ArgumentException($"Unsupported kind: {bondQsNamespaceElement.Kind}");
            }
        }

        private static SyntaxTree.QsQualifiedName ToSyntaxTreeObject(this QsQualifiedName bondQsQualifiedName)
        {
            return new SyntaxTree.QsQualifiedName(
                bondQsQualifiedName.Name.ToNonNullable(),
                bondQsQualifiedName.Namespace.ToNonNullable());
        }
    }
}
