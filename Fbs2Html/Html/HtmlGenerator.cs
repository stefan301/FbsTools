using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FbsParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fbs2Html
{
	internal class HtmlGenerator : FbsBaseListener
	{
		private SymbolTable _symbolTable;
		private StringBuilder _output = new StringBuilder();
		private string _rootFolder = string.Empty;
		private Action<string, string> _writeAction = null;

		protected readonly TextWriter ErrorOutput;
		public HtmlGenerator(SymbolTable symbolTable, TextWriter errorOutput, string inputRootFolder, Action<string, string> writeAction)
		{
			_symbolTable = symbolTable;
			ErrorOutput = errorOutput;
			_rootFolder = inputRootFolder.Last() == Path.DirectorySeparatorChar ? inputRootFolder : inputRootFolder + Path.DirectorySeparatorChar;
			_writeAction = writeAction;

		}

		private string GetHtmlFilename( string fbsFileName )
		{
			return Path.ChangeExtension(fbsFileName, ".html");
		}

		private string GetPathRelativeToCurrentFile( string absolutePath )
		{
			return MakeRelativePath(CurrentFilename, absolutePath);
		}

		private string GetPathRelativeToRootFolder(string absolutePath)
		{
			return MakeRelativePath(_rootFolder, absolutePath);
		}

		private string CssFilename => Path.Combine(_rootFolder, "style.css");
		private string StickyHeaderFilename => Path.Combine(_rootFolder, "stickyHeader.js");
		public string IndexFilename => Path.Combine(_rootFolder, "index.html");
		public string Content => _output.ToString();

		public void WriteIndex()
		{
			_output.Clear();


			_output.Append($@"<!DOCTYPE html>
<html>
<head>
  <link rel='stylesheet' href='{GetPathRelativeToRootFolder(CssFilename)}'>
</head>
  <title>{CurrentFilename}</title>
  <body>
  <div class='header' id='pageHeader'>
    <span class='header'>Overview of Flatbuffer Schemas in {_rootFolder}</span>
  </div>
  <script src='{GetPathRelativeToRootFolder(StickyHeaderFilename)}'></script>

");

			_output.AppendLine("  <h2>Namespaces:</h2>");

			_output.AppendLine("  <ul id='myUL'>");

			foreach (var ns in _symbolTable.Namespaces)
			{
				_output.AppendLine($"    <li><span class='caret'>{ns}</span>");
				_output.AppendLine("      <ul class='nested'>");

				Action<string, Func<TypeDeclaration, bool>> ListTypeDeclarations = (name, func) =>
				{
					var result = _symbolTable.TypeDeclarations.Select(p => p.Value).Where(d => d.Namespace == ns && func(d)).OrderBy(d => d.Name).ToList();
					if (result.Count > 0)
					{
						_output.AppendLine($"      <li><span class='caret'>{name}</span>");
						_output.AppendLine("        <ul class='nested'>");

						foreach (var typeDeclaration in result)
						{
							var linkTarget = $"{ GetPathRelativeToRootFolder(GetHtmlFilename(typeDeclaration.FileName))}#{typeDeclaration.Name}";

							_output.AppendLine($"          <li><a href='{linkTarget}'><span class='type_reference'>{typeDeclaration.FullyQualifiedName}</span></a></li>");

						}

						_output.AppendLine("        </ul>");
						_output.AppendLine("      </li>");
					}
				};

				ListTypeDeclarations("Root types:", d => d.IsRoot);

				ListTypeDeclarations("enums:", d => d.Type == TypeDeclaration.Kind.Enum);
				ListTypeDeclarations("structs:", d => d.Type == TypeDeclaration.Kind.Struct);
				ListTypeDeclarations("tables:", d => d.Type == TypeDeclaration.Kind.Table);
				ListTypeDeclarations("unions:", d => d.Type == TypeDeclaration.Kind.Union);

				_output.AppendLine("      </ul>");
				_output.AppendLine("    </li>");
			}

			_output.AppendLine("  </ul>");

			_output.Append(@"
  <script src='tree.js'></script>
  </body>
</html>");

			_writeAction("index.html", Content);
		}

		public override void ExitSchema([NotNull] FlatBuffersParser.SchemaContext context)
		{
			_output.Clear();

			_output.Append($@"<!DOCTYPE html>
<html>
<head>
<link rel='stylesheet' href='{GetPathRelativeToCurrentFile(CssFilename)}'>
</head>
<title>{GetPathRelativeToRootFolder(CurrentFilename)}</title>
<body>
<!-- <div class='header' id='pageHeader'>
  <a class='header' href='{GetPathRelativeToCurrentFile(IndexFilename)}'>Overview</a>
  <span class='header'>{GetPathRelativeToRootFolder(CurrentFilename)}</span>
</div>
<script src='{GetPathRelativeToCurrentFile(StickyHeaderFilename)}'></script> -->
<h2>{GetPathRelativeToRootFolder(CurrentFilename)}</h2>
<pre>
<code>
");
			foreach (var token in Tokens.GetTokens())
			{
				var htmlToken = token as HtmlCommonToken;

				switch (token.Type)
				{
					case FlatBuffersParser.BLOCK_COMMENT:
					case FlatBuffersParser.COMMENT:
						_output.Append($"<span class='comment'>{token.Text}</span>");
						break;

					case FlatBuffersParser.BASE_TYPE_NAME:
						_output.Append($"<span class='base_type_name'>{token.Text}</span>");
						break;

					case FlatBuffersParser.STRING_CONSTANT:
						if (htmlToken != null && htmlToken.TokenType == TokenType.IncludeFileName && !string.IsNullOrEmpty(htmlToken.LinkTarget) )
						{
							_output.Append($"<a href='{htmlToken.LinkTarget}'><span class='include_filename'>{token.Text}</span></a>");
						}
						else if(htmlToken != null && htmlToken.TokenType == TokenType.AttributeDeclaration && !string.IsNullOrEmpty(htmlToken.LinkId))
						{
							_output.Append($"<a id='{htmlToken.LinkId}'><span class='attribute_declaration'>{token.Text}</span></a>");
						}
						else
						{
							_output.Append($"<span class='string_constant'>{token.Text}</span>");
						}
						break;

					case FlatBuffersParser.ATTRIBUTE:
					case FlatBuffersParser.ENUM:
					case FlatBuffersParser.FILE_EXTENSION:
					case FlatBuffersParser.FILE_IDENTIFIER:
					case FlatBuffersParser.INCLUDE:
					case FlatBuffersParser.NATIVE_INCLUDE:
					case FlatBuffersParser.NAMESPACE:
					case FlatBuffersParser.ROOT_TYPE:
					case FlatBuffersParser.RPC_SERVICE:
					case FlatBuffersParser.STRUCT:
					case FlatBuffersParser.TABLE:
					case FlatBuffersParser.UNION:
						_output.Append($"<span class='keywords'>{token.Text}</span>");
						break;

					case FlatBuffersParser.IDENT:
						if ( htmlToken != null && htmlToken.TokenType == TokenType.TypeDeclaration && !string.IsNullOrEmpty(htmlToken.LinkId))
						{
							_output.Append($"<a id='{htmlToken.LinkId}'><span class='type_declaration'>{token.Text}</span></a>");
						}
						else if (htmlToken != null && htmlToken.TokenType == TokenType.TypeReference && !string.IsNullOrEmpty(htmlToken.LinkTarget))
						{
							_output.Append($"<a href='{htmlToken.LinkTarget}'><span class='type_reference'>{token.Text}</span></a>");
						}
						else if (htmlToken != null && htmlToken.TokenType == TokenType.AttributeReference && !string.IsNullOrEmpty(htmlToken.LinkTarget))
						{
							_output.Append($"<a href='{htmlToken.LinkTarget}'><span class='attribute_reference'>{token.Text}</span></a>");
						}
						else
						{
							_output.Append($"<span class='identifier'>{token.Text}</span>");
						}
						break;

					case FlatBuffersParser.Eof:
						break;

					default:
						_output.Append(token.Text);
						break;
				}
			}
			_output.Append(@"
</code>
</pre>
</body>
</html>");

			_writeAction(GetPathRelativeToRootFolder(GetHtmlFilename(CurrentFilename)), Content);
		}

		public override void EnterInclude_([NotNull] FlatBuffersParser.Include_Context context)
		{
			if(context.STRING_CONSTANT().Payload is HtmlCommonToken fileNameToken )
			{
				fileNameToken.TokenType = TokenType.IncludeFileName;

				var filename = context.STRING_CONSTANT().Symbol.Text.Trim(new char[] { '"' });

				string fullPath;
				if (_symbolTable.Files.TryGetValue(filename, out fullPath))
					fileNameToken.LinkTarget = GetPathRelativeToCurrentFile( GetHtmlFilename(fullPath) );
			}
		}

		public override void EnterAttribute_decl([NotNull] FlatBuffersParser.Attribute_declContext context)
		{
			if( context.STRING_CONSTANT().Payload is HtmlCommonToken attributeNameToken )
			{
				attributeNameToken.TokenType = TokenType.AttributeDeclaration;

				var attributeName = context.STRING_CONSTANT().Symbol.Text.Trim(new char[] { '"' });

				attributeNameToken.LinkId = $"{attributeName}";
			}
		}
		public override void EnterEnum_decl([NotNull] FlatBuffersParser.Enum_declContext context)
		{
			OnTypeDeclaration(context.identifier());
		}

		public override void EnterType_decl([NotNull] FlatBuffersParser.Type_declContext context)
		{
			OnTypeDeclaration(context.identifier());
		}
		public override void EnterUnion_decl([NotNull] FlatBuffersParser.Union_declContext context)
		{
			OnTypeDeclaration(context.identifier());
		}

		public override void EnterType_([NotNull] FlatBuffersParser.Type_Context context)
		{
			if (context.ns_ident() != null)
			{
				var typeDeclaration = FindType(context.ns_ident());
				if (typeDeclaration != null)
				{
					foreach (var identifier in context.ns_ident().identifier())
					{
						OnTypeReference(identifier, typeDeclaration);
					}
				}
			}
		}

		public override void EnterUnionval_with_opt_alias([NotNull] FlatBuffersParser.Unionval_with_opt_aliasContext context)
		{
			foreach (var ns_ident in context.ns_ident())
			{
				var typeDeclaration = FindType(ns_ident);
				if (typeDeclaration != null)
				{
					foreach (var identifier in ns_ident.identifier())
					{
						OnTypeReference(identifier, typeDeclaration);
					}
				}
			}
		}

		public override void EnterRoot_decl([NotNull] FlatBuffersParser.Root_declContext context)
		{
			var name = $"{CurrentNamespace}.{context.identifier().IDENT().Symbol.Text}";

			if (_symbolTable.TypeDeclarations.TryGetValue(name, out var typeDeclaration))
			{
				OnTypeReference(context.identifier(), typeDeclaration);
			}
			else
			{
				ErrorOutput.WriteLine($"HtmlGenerator.EnterRoot_decl: Type {name} not found.");
			}
		}

		public override void EnterMetadata([NotNull] FlatBuffersParser.MetadataContext context)
		{
			if (context.commasep_ident_with_opt_single_value() == null)
				return;

			foreach( var identWithOptSingleValueToken in context.commasep_ident_with_opt_single_value().ident_with_opt_single_value() )
			{
				var identifier = identWithOptSingleValueToken.identifier();
				if(identifier.IDENT() != null && _symbolTable.AttributeDeclarations.TryGetValue(identifier.IDENT().Symbol.Text, out var attributeDeclaration) )
				{
					OnAttributeReference(identifier, attributeDeclaration);
				}
			}
		}
		private void OnTypeDeclaration( FlatBuffersParser.IdentifierContext context)
		{
			if (context.IDENT().Payload is HtmlCommonToken typeDeclarationToken)
			{
				typeDeclarationToken.TokenType = TokenType.TypeDeclaration;
				typeDeclarationToken.LinkId = $"{typeDeclarationToken.Text}";
			}
		}
		private void OnTypeReference( FlatBuffersParser.IdentifierContext context, TypeDeclaration typeDeclaration )
		{
			if (context.IDENT().Payload is HtmlCommonToken typeReference)
			{
				typeReference.TokenType = TokenType.TypeReference;
				typeReference.LinkTarget = $"{GetPathRelativeToCurrentFile( GetHtmlFilename(typeDeclaration.FileName) )}#{typeReference.Text}";
			}
		}
		private void OnAttributeReference(FlatBuffersParser.IdentifierContext context, AttributeDeclaration attributeDeclaration)
		{
			if (context.IDENT().Payload is HtmlCommonToken attributeReference)
			{
				attributeReference.TokenType = TokenType.AttributeReference;
				attributeReference.LinkTarget = $"{GetPathRelativeToCurrentFile(GetHtmlFilename(attributeDeclaration.FileName))}#{attributeReference.Text}";
			}
		}

		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static String MakeRelativePath(String fromPath, String toPath)
		{
			if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
			if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

			Uri fromUri = new Uri(fromPath);
			Uri toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

		TypeDeclaration FindType([NotNull] FlatBuffersParser.Ns_identContext context)
		{
			var identifier = context.identifier();

			var namespaces = CurrentNamespaceDeclContext.identifier().Select(t => t.IDENT().Symbol.Text).ToList();

			var last = String.Join(".", identifier.Select(t => t.IDENT().Symbol.Text));

			for (int i = namespaces.Count; i >= 0; i--)
			{
				var ns = string.Join(".", namespaces.Take(i));
				var name = string.IsNullOrEmpty(ns) ? last : string.Join(".", ns, last);

				if (_symbolTable.TypeDeclarations.TryGetValue(name, out var typeDeclaration))
					return typeDeclaration;
			}

			ErrorOutput.WriteLine($"Type {last} not found.");
			return null;
		}
	}
}
