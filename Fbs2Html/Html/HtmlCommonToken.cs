using Antlr4.Runtime;
using System;

namespace Fbs2Html
{
	internal enum TokenType
	{
		Undefined,
		IncludeFileName,
		AttributeDeclaration,
		AttributeReference,
		TypeDeclaration,
		TypeReference
	}
	internal class HtmlCommonToken : CommonToken
	{
		public TokenType TokenType { get; set; } = TokenType.Undefined;
		public string LinkId { get; set; } = string.Empty;
		public string LinkTarget { get; set; } = String.Empty;

		public HtmlCommonToken(int type) : base(type)
		{
		}

		public HtmlCommonToken(IToken oldToken) : base(oldToken)
		{
		}

		public HtmlCommonToken(int type, string text) : base(type, text)
		{
		}

		public HtmlCommonToken(Tuple<ITokenSource, ICharStream> source, int type, int channel, int start, int stop) : base(source, type, channel, start, stop)
		{
		}
	}
}
