using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FbsParser;
using System.IO;

namespace Fbs2Html
{
	internal class SymbolTableListener : FbsBaseListener
	{
		private SymbolTable _symbolTable;

		public SymbolTableListener(SymbolTable symbolTable)
		{
			this._symbolTable = symbolTable;
		}

		public override void EnterSchema([NotNull] FlatBuffersParser.SchemaContext context)
		{
			_symbolTable.Files.Add(Path.GetFileName(CurrentFilename), CurrentFilename);
		}

		public override void EnterNamespace_decl([NotNull] FlatBuffersParser.Namespace_declContext context)
		{
			base.EnterNamespace_decl(context);
			_symbolTable.Namespaces.Add(CurrentNamespace);
		}

		public override void EnterAttribute_decl([NotNull] FlatBuffersParser.Attribute_declContext context)
		{
			var attributeName = context.STRING_CONSTANT().Symbol.Text.Trim(new char[] { '"' });

			var attributeDeclaration = new AttributeDeclaration(attributeName, CurrentFilename);
			_symbolTable.AttributeDeclarations.Add(attributeName, attributeDeclaration);
		}

		public override void EnterType_decl([NotNull] FlatBuffersParser.Type_declContext context)
		{
			var identifierToken = context.identifier().IDENT().Payload as CommonToken;

			var kind = context.STRUCT() != null ? TypeDeclaration.Kind.Struct : TypeDeclaration.Kind.Table;

			var typeDeclation = new TypeDeclaration(kind, CurrentNamespace, identifierToken.Text, CurrentFilename);
			_symbolTable.TypeDeclarations.Add(typeDeclation.FullyQualifiedName, typeDeclation);
		}

		public override void EnterUnion_decl([NotNull] FlatBuffersParser.Union_declContext context)
		{
			var identifierToken = context.identifier().IDENT().Payload as CommonToken;

			var typeDeclation = new TypeDeclaration(TypeDeclaration.Kind.Union, CurrentNamespace, identifierToken.Text, CurrentFilename);
			_symbolTable.TypeDeclarations.Add(typeDeclation.FullyQualifiedName, typeDeclation);
		}

		public override void EnterEnum_decl([NotNull] FlatBuffersParser.Enum_declContext context)
		{
			var identifierToken = context.identifier().IDENT().Payload as CommonToken;

			var typeDeclation = new TypeDeclaration(TypeDeclaration.Kind.Enum, CurrentNamespace, identifierToken.Text, CurrentFilename);
			_symbolTable.TypeDeclarations.Add(typeDeclation.FullyQualifiedName, typeDeclation);
		}
		public override void EnterRoot_decl([NotNull] FlatBuffersParser.Root_declContext context)
		{
			var name = $"{CurrentNamespace}.{context.identifier().IDENT().Symbol.Text}";

			if (_symbolTable.TypeDeclarations.TryGetValue(name, out var typeDeclaration))
			{
				typeDeclaration.IsRoot = true;
			}
		}
	}
}
