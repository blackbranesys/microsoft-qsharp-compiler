// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Microsoft.Quantum.QsCompiler.DataTypes;
// TODO: Add documentation.

using QsDocumentation = System.Linq.ILookup<Microsoft.Quantum.QsCompiler.DataTypes.NonNullable<string>, System.Collections.Immutable.ImmutableArray<string>>;
using CompilerResolvedTypeOperation = System.Tuple<System.Tuple<Microsoft.Quantum.QsCompiler.SyntaxTree.ResolvedType, Microsoft.Quantum.QsCompiler.SyntaxTree.ResolvedType>, Microsoft.Quantum.QsCompiler.SyntaxTree.CallableInformation>;

namespace Microsoft.Quantum.QsCompiler.BondSchemas
{
    // TODO: Add documentation.
    public static class Extensions
    {
        // TODO: Add documentation.
        // TODO: Check that values that might be null are being correctly represented.
        public static QsCompilation CreateBondCompilation(SyntaxTree.QsCompilation qsCompilation) =>
            new QsCompilation
            {
                Namespaces = qsCompilation.Namespaces.Select(n => n.ToBondSchema()).ToList(),
                EntryPoints = qsCompilation.EntryPoints.Select(e => e.ToBondSchema()).ToList()
            };

        // TODO: Add documentation.
        // TODO: Check that values that might be null are being correctly represented.
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

        private static CallableInformation ToBondSchema(this SyntaxTree.CallableInformation callableInformation) =>
            new CallableInformation
            {
                // TODO: Implement Characteristics.
                // TODO: Implement InferredInformation.
            };

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
                ArgumentTuple = qsCallable.ArgumentTuple.ToBondSchema(),
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

        private static LocalVariableDeclaration<QsLocalSymbol> ToBondSchema(
            this SyntaxTree.LocalVariableDeclaration<SyntaxTree.QsLocalSymbol> localVariableDeclaration) =>
            localVariableDeclaration.ToBondSchemaGeneric(ToBondSchema);

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

        private static QsTuple<LocalVariableDeclaration<QsLocalSymbol>> ToBondSchema(
            this SyntaxTokens.QsTuple<SyntaxTree.LocalVariableDeclaration<SyntaxTree.QsLocalSymbol>> localVariableDeclaration) =>
            localVariableDeclaration.ToBondSchemaGeneric(ToBondSchema);

        private static QsTypeKindDetails<ResolvedType, UserDefinedType, QsTypeParameter, CallableInformation> ToBondSchema(
            this SyntaxTokens.QsTypeKind<SyntaxTree.ResolvedType, SyntaxTree.UserDefinedType, SyntaxTree.QsTypeParameter, SyntaxTree.CallableInformation> qsTypeKind) =>
            qsTypeKind.ToBondSchemaGeneric
                <ResolvedType,
                 UserDefinedType,
                 QsTypeParameter,
                 CallableInformation,
                 SyntaxTree.ResolvedType,
                 SyntaxTree.UserDefinedType,
                 SyntaxTree.QsTypeParameter,
                 SyntaxTree.CallableInformation>(
            ToBondSchema,
            ToBondSchema,
            ToBondSchema,
            ToBondSchema);

        private static QsTypeParameter ToBondSchema(this SyntaxTree.QsTypeParameter qsTypeParameter) =>
            new QsTypeParameter
            {
                Origin = qsTypeParameter.Origin.ToBondSchema(),
                TypeName = qsTypeParameter.TypeName.Value,
                Range = qsTypeParameter.Range.IsNull ? null : qsTypeParameter.Range.Item.ToBondSchema()
            };

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

        private static ResolvedType ToBondSchema(this SyntaxTree.ResolvedType resolvedType) =>
            new ResolvedType
            {
                TypeKind = resolvedType.Resolution.ToBondSchema()
            };

        private static UserDefinedType ToBondSchema(this SyntaxTree.UserDefinedType userDefinedType) =>
            new UserDefinedType
            {
                Namespace = userDefinedType.Namespace.Value,
                Name = userDefinedType.Name.Value,
                Range = userDefinedType.Range.IsNull ? null : userDefinedType.Range.Item.ToBondSchema()
            };

        private static LocalVariableDeclaration<BondType> ToBondSchemaGeneric<BondType, CompilerType>(
            this SyntaxTree.LocalVariableDeclaration<CompilerType> localVariableDeclaration,
            Func<CompilerType, BondType> toBondSchema) =>
            new LocalVariableDeclaration<BondType>
            {
                VariableName = toBondSchema(localVariableDeclaration.VariableName),
                Type = localVariableDeclaration.Type.ToBondSchema(),
                // TODO: Implement InferredInformation.
                Position = localVariableDeclaration.Position.IsNull ?
                    null :
                    localVariableDeclaration.Position.Item.ToBondSchema(),
                Range = localVariableDeclaration.Range.ToBondSchema()
            };

        private static QsTuple<BondType> ToBondSchemaGeneric<BondType, CompilerType>(
            this SyntaxTokens.QsTuple<CompilerType> qsTuple,
            Func<CompilerType, BondType> toBondSchema)
        {
            CompilerType item = default;
            ImmutableArray<SyntaxTokens.QsTuple<CompilerType>> items;
            if (qsTuple.TryGetQsTupleItem(ref item))
            {
                return new QsTuple<BondType>
                {
                    Kind = QsTupleKind.QsTupleItem,
                    Item = toBondSchema(item)
                };
            }
            else if (qsTuple.TryGetQsTuple(ref items))
            {
                return new QsTuple<BondType>
                {
                    Kind = QsTupleKind.QsTuple,
                    Items = items.Select(i => i.ToBondSchemaGeneric(toBondSchema)).ToList()
                };
            }
            else
            {
                throw new ArgumentException($"Unsupported QsTuple kind {qsTuple}");
            }
        }

        private static QsTypeKind ToBondSchemaGeneric<
            CompilerDataType,
            CompilerUdtType,
            CompilerTParamType,
            CompilerCharacteristicsType>(
            this SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType> qsTypeKind) =>
            qsTypeKind.Tag switch
            {
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.ArrayType => QsTypeKind.ArrayType,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.BigInt => QsTypeKind.BigInt,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Bool => QsTypeKind.Bool,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Double => QsTypeKind.Double,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Function => QsTypeKind.Function,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Int => QsTypeKind.Int,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.InvalidType => QsTypeKind.InvalidType,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.MissingType => QsTypeKind.MissingType,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Operation => QsTypeKind.Operation,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Pauli => QsTypeKind.Pauli,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Qubit => QsTypeKind.Qubit,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Range => QsTypeKind.Range,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Result => QsTypeKind.Result,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.String => QsTypeKind.String,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.TupleType => QsTypeKind.TupleType,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.TypeParameter => QsTypeKind.TypeParameter,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.UnitType => QsTypeKind.UnitType,
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.UserDefinedType => QsTypeKind.UserDefinedType,
                _ => throw new ArgumentException($"Unsupported QsTypeKind: {qsTypeKind.Tag}")
            };

        private static QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType> ToBondSchemaGeneric
            <BondDataType,
             BondUdtType,
             BondTParamType,
             BondCharacteristicsType,
             CompilerDataType,
             CompilerUdtType,
             CompilerTParamType,
             CompilerCharacteristicsType>(
                this SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType> qsTypeKind,
                Func<CompilerDataType, BondDataType> dataTypeTranslator,
                Func<CompilerUdtType, BondUdtType> udtTypeTranslator,
                Func<CompilerTParamType, BondTParamType> tParamTypeTranslator,
                Func<CompilerCharacteristicsType, BondCharacteristicsType> characteristicsTypeTranslator)
            where BondDataType : class
            where BondUdtType : class
            where BondTParamType : class
            where BondCharacteristicsType : class
            where CompilerDataType : class
            where CompilerUdtType : class
            where CompilerTParamType : class
            where CompilerCharacteristicsType : class
        {
            
            BondDataType bondArrayType = null;
            QsTypeKindFunction<BondDataType> bondFunction = null;
            QsTypeKindOperation<BondDataType, BondCharacteristicsType> bondOperation = null;
            List<BondDataType> bondTupleType = null;
            BondTParamType bondTypeParameter = null;
            BondUdtType bondUserDefinedType = null;
            CompilerDataType compilerArrayType = null;
            Tuple<CompilerDataType, CompilerDataType> compilerFunction = null;
            Tuple<Tuple<CompilerDataType, CompilerDataType>, CompilerCharacteristicsType> compilerOperation = null;
            ImmutableArray<CompilerDataType> compilerTupleType;
            CompilerTParamType compilerTyperParameter = null;
            CompilerUdtType compilerUdtType = null;
            if (qsTypeKind.TryGetArrayType(ref compilerArrayType))
            {
                bondArrayType = dataTypeTranslator(compilerArrayType);
            }
            else if (qsTypeKind.TryGetFunction(ref compilerFunction))
            {
                bondFunction = new QsTypeKindFunction<BondDataType>
                {
                    DataA = dataTypeTranslator(compilerFunction.Item1),
                    DataB = dataTypeTranslator(compilerFunction.Item2)
                };
            }
            else if (qsTypeKind.TryGetOperation(ref compilerOperation))
            {
                bondOperation = new QsTypeKindOperation<BondDataType, BondCharacteristicsType>
                {
                    DataA = dataTypeTranslator(compilerOperation.Item1.Item1),
                    DataB = dataTypeTranslator(compilerOperation.Item1.Item2),
                    Characteristics = characteristicsTypeTranslator(compilerOperation.Item2)
                };
            }
            else if (qsTypeKind.TryGetTupleType(ref compilerTupleType))
            {
                bondTupleType = compilerTupleType.Select(t => dataTypeTranslator(t)).ToList();
            }
            else if (qsTypeKind.TryGetTypeParameter(ref compilerTyperParameter))
            {
                bondTypeParameter = tParamTypeTranslator(compilerTyperParameter);
            }
            else if (qsTypeKind.TryGetUserDefinedType(ref compilerUdtType))
            {
                bondUserDefinedType = udtTypeTranslator(compilerUdtType);
            }

            // TODO: Implement additional kinds.

            var bondQsTypeKindDetails = qsTypeKind.Tag switch
            {
                var tag when
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.BigInt ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Bool ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Double ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Int ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.InvalidType ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.MissingType ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Pauli ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Qubit ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Range ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Result ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.String ||
                    tag == SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.UnitType =>
                        new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                        {
                            Kind = qsTypeKind.ToBondSchemaGeneric()
                        },
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.ArrayType =>
                    new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                    {
                        Kind = QsTypeKind.ArrayType,
                        ArrayType = bondArrayType ?? throw new InvalidOperationException($"ArrayType cannot be null when Kind is {QsTypeKind.ArrayType}")
                    },
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Function =>
                    new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                    {
                        Kind = QsTypeKind.Function,
                        Function = bondFunction ?? throw new InvalidOperationException($"Function cannot be null when Kind is {QsTypeKind.Function}")
                    },
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.Operation =>
                    new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                    {
                        Kind = QsTypeKind.Operation,
                        Operation = bondOperation ?? throw new InvalidOperationException($"Operation cannot be null when Kind is {QsTypeKind.Operation}")
                    },
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.TupleType =>
                    new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                    {
                        Kind = QsTypeKind.TupleType,
                        TupleType = bondTupleType ?? throw new InvalidOperationException($"TupleType cannot be null when Kind is {QsTypeKind.TupleType}")
                    },
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.TypeParameter =>
                    new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                    {
                        Kind = QsTypeKind.TypeParameter,
                        TypeParameter = bondTypeParameter ?? throw new InvalidOperationException($"TypeParameter cannot be null when Kind is {QsTypeKind.TypeParameter}")
                    },
                SyntaxTokens.QsTypeKind<CompilerDataType, CompilerUdtType, CompilerTParamType, CompilerCharacteristicsType>.Tags.UserDefinedType =>
                    new QsTypeKindDetails<BondDataType, BondUdtType, BondTParamType, BondCharacteristicsType>
                    {
                        Kind = QsTypeKind.UserDefinedType,
                        UserDefinedType = bondUserDefinedType ?? throw new InvalidOperationException($"UserDefinedType cannot be null when Kind is {QsTypeKind.UserDefinedType}")
                    },
                _ => throw new ArgumentException($"Unsupported QsTypeKind: {qsTypeKind.Tag}")
            };

            return bondQsTypeKindDetails;
        }


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
                location: bondQsCallable.Location.ToQsNullable(),
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

        private static SyntaxTree.QsCustomType ToCompilerObject(this QsCustomType bondQsCustomType) =>
            new SyntaxTree.QsCustomType(
                fullName: bondQsCustomType.FullName.ToCompilerObject(),
                // TODO: Implement Attributes.
                attributes: Array.Empty<SyntaxTree.QsDeclarationAttribute>().ToImmutableArray(),
                modifiers: bondQsCustomType.Modifiers.ToCompilerObject(),
                sourceFile: bondQsCustomType.SourceFile.ToNonNullable(),
                location: bondQsCustomType.Location.ToQsNullable(),
                // TODO: Implement Type.
                type: default,
                // TODO: Implement TypeItems.
                typeItems: default,
                documentation: bondQsCustomType.Documentation.ToImmutableArray(),
                comments: bondQsCustomType.Comments.ToCompilerObject());

        private static SyntaxTree.QsDeclarationAttribute ToCompilerObject(this QsDeclarationAttribute bondQsDeclarationAttribute) =>
            new SyntaxTree.QsDeclarationAttribute(
                typeId: bondQsDeclarationAttribute.TypeId.ToQsNullable(),
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
            new SyntaxTree.QsLocation(
                offset: bondQsLocation.Offset.ToCompilerObject(),
                range: bondQsLocation.Range.ToCompilerObject());

        private static SyntaxTree.QsNamespace ToCompilerObject(this QsNamespace bondQsNamespace) =>
            new SyntaxTree.QsNamespace(
                name: bondQsNamespace.Name.ToNonNullable(),
                elements: bondQsNamespace.Elements.Select(e => e.ToCompilerObject()).ToImmutableArray(),
                documentation: bondQsNamespace.Documentation.ToLookup(
                    p => p.FileName.ToNonNullable(),
                    p => p.DocumentationItems.ToImmutableArray()));

        private static SyntaxTree.QsNamespaceElement ToCompilerObject(this QsNamespaceElement bondQsNamespaceElement)
        {
            if (bondQsNamespaceElement.Kind == QsNamespaceElementKind.QsCallable)
            {
                return SyntaxTree.QsNamespaceElement.NewQsCallable(bondQsNamespaceElement.Callable.ToCompilerObject());
            }
            else if (bondQsNamespaceElement.Kind == QsNamespaceElementKind.QsCustomType)
            {
                return SyntaxTree.QsNamespaceElement.NewQsCustomType(bondQsNamespaceElement.CustomType.ToCompilerObject());
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
                range: userDefinedType.Range.ToQsNullable());

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

        private static QsNullable<DataTypes.Range> ToQsNullable(this Range range) =>
            range != null ?
                range.ToCompilerObject().ToQsNullable() :
                QsNullable<DataTypes.Range>.Null;

        private static QsNullable<DataTypes.Range> ToQsNullable(this DataTypes.Range range) =>
            QsNullable<DataTypes.Range>.NewValue(range);

        private static QsNullable<SyntaxTree.QsLocation> ToQsNullable(this QsLocation qsLocation) =>
            qsLocation != null ?
                qsLocation.ToCompilerObject().ToQsNullable() :
                QsNullable<SyntaxTree.QsLocation>.Null;

        private static QsNullable<SyntaxTree.QsLocation> ToQsNullable(this SyntaxTree.QsLocation qsLocation) =>
            QsNullable<SyntaxTree.QsLocation>.NewValue(qsLocation);

        private static QsNullable<SyntaxTree.UserDefinedType> ToQsNullable(this UserDefinedType userDefinedType) =>
            userDefinedType != null ?
                userDefinedType.ToCompilerObject().ToQsNullable() :
                QsNullable<SyntaxTree.UserDefinedType>.Null;

        private static QsNullable<SyntaxTree.UserDefinedType> ToQsNullable(this SyntaxTree.UserDefinedType userDefinedType) =>
            QsNullable<SyntaxTree.UserDefinedType>.NewValue(userDefinedType);
    }
}
