using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FbsParser;
using System;
using System.IO;

namespace Fbs2Html
{
	internal class SymbolTableListener : FbsBaseListener
	{
		private SymbolTable _symbolTable;

		protected readonly TextWriter ErrorOutput;

		public SymbolTableListener(SymbolTable symbolTable, TextWriter errorOutput)
		{
			this._symbolTable = symbolTable;
			ErrorOutput = errorOutput;
		}

		public override void EnterSchema([NotNull] FlatBuffersParser.SchemaContext context)
		{
			if (string.IsNullOrEmpty(CurrentFilename))
			{
				ErrorOutput.WriteLine($"SymbolTableListener.EnterSchema: CurrentFilename not set.");
				return;
			}

			string filename = Path.GetFileName(CurrentFilename);

			try
			{
				_symbolTable.Files.Add(filename, CurrentFilename);
			}
			catch(Exception)
			{
				ErrorOutput.WriteLine($"SymbolTableListener.EnterSchema: failed to add file {filename}");
			}
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
			try
			{
				_symbolTable.AttributeDeclarations.Add(attributeName, attributeDeclaration);
			}
			catch (Exception)
			{
				ErrorOutput.WriteLine($"SymbolTableListener.EnterAttribute_decl: failed to add Attribute {attributeName}");
			}
		}

		public override void EnterType_decl([NotNull] FlatBuffersParser.Type_declContext context)
		{
			var identifierToken = context.identifier().IDENT().Payload as CommonToken;

			var kind = context.STRUCT() != null ? TypeDeclaration.Kind.Struct : TypeDeclaration.Kind.Table;

			var typeDeclation = new TypeDeclaration(kind, CurrentNamespace, identifierToken.Text, CurrentFilename);

			try
			{
				_symbolTable.TypeDeclarations.Add(typeDeclation.FullyQualifiedName, typeDeclation);
			}
			catch (Exception)
			{
				ErrorOutput.WriteLine($"SymbolTableListener.EnterType_decl: failed to add Type {identifierToken.Text}");
			}
		}

		public override void EnterUnion_decl([NotNull] FlatBuffersParser.Union_declContext context)
		{
			var identifierToken = context.identifier().IDENT().Payload as CommonToken;

			var typeDeclation = new TypeDeclaration(TypeDeclaration.Kind.Union, CurrentNamespace, identifierToken.Text, CurrentFilename);

			try
			{
				_symbolTable.TypeDeclarations.Add(typeDeclation.FullyQualifiedName, typeDeclation);
			}
			catch (Exception)
			{
				ErrorOutput.WriteLine($"SymbolTableListener.EnterUnion_decl: failed to add Union {identifierToken.Text}");
			}
		}

		public override void EnterEnum_decl([NotNull] FlatBuffersParser.Enum_declContext context)
		{
			var identifierToken = context.identifier().IDENT().Payload as CommonToken;

			var typeDeclation = new TypeDeclaration(TypeDeclaration.Kind.Enum, CurrentNamespace, identifierToken.Text, CurrentFilename);

			try
			{
				_symbolTable.TypeDeclarations.Add(typeDeclation.FullyQualifiedName, typeDeclation);
			}
			catch (Exception)
			{
				ErrorOutput.WriteLine($"SymbolTableListener.EnterEnum_decl: failed to add Enum {identifierToken.Text}");
			}
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
