﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Quantum.QsCompiler.DataTypes;

namespace Microsoft.Quantum.QsCompiler.BondSchemas
{
    // TODO: Add documentation.
    using QsDocumentation = ILookup<NonNullable<string>, ImmutableArray<string>>;

    // TODO: Add documentation.
    public static class Extensions
    {
        // TODO: Add documentation.
        public static QsCompilation CreateBondCompilation(SyntaxTree.QsCompilation qsCompilation) =>
            new QsCompilation
            {
                Namespaces = qsCompilation.Namespaces.Select(n => n.ToBondSchema()).ToList(),
                EntryPoints = qsCompilation.EntryPoints.Select(e => e.ToBondSchema()).ToList()
            };

        // TODO: Add documentation.
        public static SyntaxTree.QsCompilation CreateQsCompilation(QsCompilation bondCompilation) =>
            new SyntaxTree.QsCompilation(
                namespaces: bondCompilation.Namespaces.Select(n => n.ToCompilerObject()).ToImmutableArray(),
                // TODO: Implement EntryPoints.
                entryPoints: Array.Empty<SyntaxTree.QsQualifiedName>().ToImmutableArray());

        private static AccessModifier ToBondSchema(this SyntaxTokens.AccessModifier accessModifier)
        {
            if (accessModifier.IsDefaultAccess)
            {
                return AccessModifier.DefaultAccess;
            }
            else if (accessModifier.IsInternal)
            {
                return AccessModifier.Internal;
            }
            else
            {
                throw new ArgumentException($"Unsupported access modifier: {accessModifier}");
            }
        }

        private static Modifiers ToBondSchema(this SyntaxTokens.Modifiers modifiers) =>
            new Modifiers
            {
                Access = modifiers.Access.ToBondSchema()
            };

        private static Position ToBondSchema(this DataTypes.Position position) =>
            new Position
            {
                Line = position.Line,
                Column = position.Column
            };

        private static QsCallable ToBondSchema(this SyntaxTree.QsCallable qsCallable) =>
            new QsCallable
            {
                Kind = qsCallable.Kind.ToBondSchema(),
                FullName = qsCallable.FullName.ToBondSchema(),
                Attributes = qsCallable.Attributes.Select(a => a.ToBondSchema()).ToList(),
                Modifiers = qsCallable.Modifiers.ToBondSchema(),
                SourceFile = qsCallable.SourceFile.Value,
                Location = qsCallable.Location.IsNull ? null : qsCallable.Location.Item.ToBondSchema(),
                Signature = qsCallable.Signature.ToBondSchema(),
                // TODO: Implement ArgumentTuple.
                // TODO: Implement Specializations.
                Documentation = qsCallable.Documentation.ToList(),
                Comments = qsCallable.Comments.ToBondSchema()
            };

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

        private static QsComments ToBondSchema(this SyntaxTree.QsComments qsComments) =>
            new QsComments
            {
                OpeningComments = qsComments.OpeningComments.ToList(),
                ClosingComments = qsComments.ClosingComments.ToList()
            };

        private static QsCustomType ToBondSchema(this SyntaxTree.QsCustomType qsCustomType) =>
            new QsCustomType
            {
                FullName = qsCustomType.FullName.ToBondSchema(),
                Attributes = qsCustomType.Attributes.Select(a => a.ToBondSchema()).ToList(),
                Modifiers = qsCustomType.Modifiers.ToBondSchema(),
                SourceFile = qsCustomType.SourceFile.Value,
                // TODO: Implement Location.
                // TODO: Implement Type.
                // TODO: Implement TypeItems.
                Documentation = qsCustomType.Documentation.ToList(),
                Comments = qsCustomType.Comments.ToBondSchema()
            };

        private static QsDeclarationAttribute ToBondSchema(this SyntaxTree.QsDeclarationAttribute qsDeclarationAttribute) =>
            new QsDeclarationAttribute
            {
                TypeId = qsDeclarationAttribute.TypeId.IsNull ? null : qsDeclarationAttribute.TypeId.Item.ToBondSchema(),
                // TODO: Implement Argument
                Offset = qsDeclarationAttribute.Offset.ToBondSchema(),
                Comments = qsDeclarationAttribute.Comments.ToBondSchema()
            };

        private static QsQualifiedName ToBondSchema(this SyntaxTree.QsQualifiedName qsQualifiedName) =>
            new QsQualifiedName
            {
                Namespace = qsQualifiedName.Namespace.Value,
                Name = qsQualifiedName.Name.Value
            };

        private static QsLocalSymbol ToBondSchema(this SyntaxTree.QsLocalSymbol qsLocalSymbol)
        {
            var validName = NonNullable<string>.New(string.Empty);
            if (qsLocalSymbol.TryGetValidName(ref validName))
            {
                return new QsLocalSymbol
                {
                    Kind = QsLocalSymbolKind.ValidName,
                    Name = validName.Value
                };
            }
            else if (qsLocalSymbol.IsInvalidName)
            {
                return new QsLocalSymbol
                {
                    Kind = QsLocalSymbolKind.InvalidName
                };
            }
            else
            {
                throw new ArgumentException($"Unsupported QsLocalSymbol {qsLocalSymbol}");
            }
        }

        private static QsLocation ToBondSchema(this SyntaxTree.QsLocation qsLocation) =>
            new QsLocation
            {
                Offset = qsLocation.Offset.ToBondSchema(),
                Range = qsLocation.Range.ToBondSchema()
            };

        private static QsNamespace ToBondSchema(this SyntaxTree.QsNamespace qsNamespace) =>
            new QsNamespace
            {
                Name = qsNamespace.Name.Value,
                Elements = qsNamespace.Elements.Select(e => e.ToBondSchema()).ToList(),
                Documentation = qsNamespace.Documentation.ToBondSchema()
            };

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

        private static LinkedList<QsSourceFileDocumentation> ToBondSchema(this QsDocumentation qsDocumentation)
        {
            var documentationList = new LinkedList<QsSourceFileDocumentation>();
            foreach (var qsSourceFileDocumentation in qsDocumentation)
            {
                foreach (var items in qsSourceFileDocumentation)
                {
                    var qsDocumentationItem = new QsSourceFileDocumentation
                    {
                        FileName = qsSourceFileDocumentation.Key.Value,
                        DocumentationItems = items.ToList()
                    };

                    documentationList.AddLast(qsDocumentationItem);
                }
            }

            return documentationList;
        }

        private static Range ToBondSchema(this DataTypes.Range range) =>
            new Range
            {
                Start = range.Start.ToBondSchema(),
                End = range.End.ToBondSchema()
            };

        private static ResolvedSignature ToBondSchema(this SyntaxTree.ResolvedSignature resolvedSignature) =>
            new ResolvedSignature
            {
                TypeParameters = resolvedSignature.TypeParameters.Select(tp => tp.ToBondSchema()).ToList()
                // TODO: Implement ArgumentType
                // TODO: Implement ReturnType
                // TODO: Implement Information
            };

        private static UserDefinedType ToBondSchema(this SyntaxTree.UserDefinedType userDefinedType) =>
            new UserDefinedType
            {
                Namespace = userDefinedType.Namespace.Value,
                Name = userDefinedType.Name.Value,
                Range = userDefinedType.Range.IsNull ? null : userDefinedType.Range.Item.ToBondSchema()
            };

        private static DataTypes.Position ToCompilerObject(this Position position) =>
            DataTypes.Position.Create(position.Line, position.Column);

        private static DataTypes.Range ToCompilerObject(this Range range) =>
            DataTypes.Range.Create(range.Start.ToCompilerObject(), range.End.ToCompilerObject());

        private static SyntaxTree.QsCallable ToCompilerObject(this QsCallable bondQsCallable) =>
            new SyntaxTree.QsCallable(
                kind: bondQsCallable.Kind.ToCompilerObject(),
                fullName: bondQsCallable.FullName.ToCompilerObject(),
                attributes: bondQsCallable.Attributes.Select(a => a.ToCompilerObject()).ToImmutableArray(),
                modifiers: bondQsCallable.Modifiers.ToCompilerObject(),
                sourceFile: bondQsCallable.SourceFile.ToNonNullable(),
                location: bondQsCallable.Location.ToCompilerObject().ToQsNullable(),
                signature: bondQsCallable.Signature.ToCompilerObject(),
                // TODO: Implement ArgumentTuple.
                argumentTuple: default,
                // TODO: Implement Specializations.
                specializations: Array.Empty<SyntaxTree.QsSpecialization>().ToImmutableArray(),
                documentation: bondQsCallable.Documentation.ToImmutableArray(),
                comments: bondQsCallable.Comments.ToCompilerObject());

        private static SyntaxTree.QsCallableKind ToCompilerObject(this QsCallableKind bondQsCallableKind) =>
            bondQsCallableKind switch
            {
                QsCallableKind.Operation => SyntaxTree.QsCallableKind.Operation,
                QsCallableKind.Function => SyntaxTree.QsCallableKind.Function,
                QsCallableKind.TypeConstructor => SyntaxTree.QsCallableKind.TypeConstructor,
                _ => throw new ArgumentException($"Unsupported Bond QsCallableKind: {bondQsCallableKind}")
            };

        private static SyntaxTree.QsComments ToCompilerObject(this QsComments bondQsComments) =>
            new SyntaxTree.QsComments(
                bondQsComments.OpeningComments.ToImmutableArray(),
                bondQsComments.ClosingComments.ToImmutableArray());

        private static SyntaxTree.QsCustomType ToSyntaxTreeObject(this QsCustomType bondQsCustomType) =>
            new SyntaxTree.QsCustomType(
                fullName: bondQsCustomType.FullName.ToCompilerObject(),
                // TODO: Implement needed extensions.
                attributes: Array.Empty<SyntaxTree.QsDeclarationAttribute>().ToImmutableArray(),
                // TODO: Get this from the bond object.
                modifiers: new SyntaxTokens.Modifiers(),
                sourceFile: bondQsCustomType.SourceFile.ToNonNullable(),
                location: bondQsCustomType.Location.ToCompilerObject().ToQsNullable(),
                // TODO: Implement this.
                type: default,
                // TODO: Implement this.
                typeItems: default,
                documentation: bondQsCustomType.Documentation.ToImmutableArray(),
                comments: bondQsCustomType.Comments.ToCompilerObject());

        private static SyntaxTree.QsDeclarationAttribute ToCompilerObject(this QsDeclarationAttribute bondQsDeclarationAttribute) =>
            new SyntaxTree.QsDeclarationAttribute(
                typeId: bondQsDeclarationAttribute.TypeId.ToCompilerObject().ToQsNullable(),
                // TODO: Implement Argument.
                argument: default,
                offset: bondQsDeclarationAttribute.Offset.ToCompilerObject(),
                comments: bondQsDeclarationAttribute.Comments.ToCompilerObject());

        private static SyntaxTree.QsLocalSymbol ToCompilerObject(this QsLocalSymbol bondQsLocalSymbol) =>
            bondQsLocalSymbol.Kind switch
            {
                QsLocalSymbolKind.ValidName => SyntaxTree.QsLocalSymbol.NewValidName(bondQsLocalSymbol.Name.ToNonNullable()),
                QsLocalSymbolKind.InvalidName => SyntaxTree.QsLocalSymbol.InvalidName,
                _ => throw new ArgumentException($"Unsupported QsLocalSymbolKind: {bondQsLocalSymbol.Kind}")
            };

        private static SyntaxTree.QsLocation ToCompilerObject(this QsLocation bondQsLocation) =>
            bondQsLocation != null ?
                new SyntaxTree.QsLocation(bondQsLocation.Offset.ToCompilerObject(), bondQsLocation.Range.ToCompilerObject()) :
                null;

        private static SyntaxTree.QsNamespace ToCompilerObject(this QsNamespace bondQsNamespace)
        {
            var elements = new List<SyntaxTree.QsNamespaceElement>();
            foreach(var bondNamespaceElement in bondQsNamespace.Elements)
            {
                elements.Add(bondNamespaceElement.ToCompilerObject());
            }

            return new SyntaxTree.QsNamespace(
                bondQsNamespace.Name.ToNonNullable(),
                elements.ToImmutableArray(),
                bondQsNamespace.Documentation.ToLookup(
                    p => p.FileName.ToNonNullable(),
                    p => p.DocumentationItems.ToImmutableArray()));
        }

        private static SyntaxTree.QsNamespaceElement ToCompilerObject(this QsNamespaceElement bondQsNamespaceElement)
        {
            if (bondQsNamespaceElement.Kind == QsNamespaceElementKind.QsCallable)
            {
                return SyntaxTree.QsNamespaceElement.NewQsCallable(bondQsNamespaceElement.Callable.ToCompilerObject());
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

        private static SyntaxTree.QsQualifiedName ToCompilerObject(this QsQualifiedName bondQsQualifiedName)
        {
            return new SyntaxTree.QsQualifiedName(
                bondQsQualifiedName.Name.ToNonNullable(),
                bondQsQualifiedName.Namespace.ToNonNullable());
        }

        private static SyntaxTree.ResolvedSignature ToCompilerObject(this ResolvedSignature bondResolvedSignature) =>
            new SyntaxTree.ResolvedSignature(
                typeParameters: bondResolvedSignature.TypeParameters.Select(tp => tp.ToCompilerObject()).ToImmutableArray(),
                // Implement ArgumentType
                argumentType: default,
                // Implement ReturnType
                returnType: default,
                // Implement Information
                information: default);

        private static SyntaxTree.UserDefinedType ToCompilerObject(this UserDefinedType userDefinedType) =>
            new SyntaxTree.UserDefinedType(
                @namespace: userDefinedType.Namespace.ToNonNullable(),
                name: userDefinedType.Name.ToNonNullable(),
                range: userDefinedType.Range == null ?
                    QsNullable<DataTypes.Range>.Null :
                    userDefinedType.Range.ToCompilerObject().ToQsNullable());

        private static SyntaxTokens.AccessModifier ToCompilerObject(this AccessModifier accessModifier) =>
        accessModifier switch
        {
            AccessModifier.DefaultAccess => SyntaxTokens.AccessModifier.DefaultAccess,
            AccessModifier.Internal => SyntaxTokens.AccessModifier.Internal,
            _ => throw new ArgumentException($"Unsupported AccessModifier: {accessModifier}")
        };

        private static SyntaxTokens.Modifiers ToCompilerObject(this Modifiers modifiers) =>
            new SyntaxTokens.Modifiers(modifiers.Access.ToCompilerObject());

        private static NonNullable<string> ToNonNullable(this string str) =>
            NonNullable<string>.New(str);

        private static QsNullable<DataTypes.Range> ToQsNullable(this DataTypes.Range range) =>
            QsNullable<DataTypes.Range>.NewValue(range);

        private static QsNullable<SyntaxTree.QsLocation> ToQsNullable(this SyntaxTree.QsLocation qsLocation) =>
            QsNullable<SyntaxTree.QsLocation>.NewValue(qsLocation);

        private static QsNullable<SyntaxTree.UserDefinedType> ToQsNullable(this SyntaxTree.UserDefinedType userDefinedType) =>
            QsNullable<SyntaxTree.UserDefinedType>.NewValue(userDefinedType);
    }
}
