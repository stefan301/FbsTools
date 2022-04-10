using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;

namespace Fbs2Html
{
	internal class HtmlCommonTokenFactory : CommonTokenFactory
	{
		public HtmlCommonTokenFactory()
		{
		}

		public HtmlCommonTokenFactory(bool copyText) : base(copyText)
		{
		}

		public override CommonToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
		{
			CommonToken commonToken = new HtmlCommonToken(source, type, channel, start, stop);
			commonToken.Line = line;
			commonToken.Column = charPositionInLine;
			if (text != null)
			{
				commonToken.Text = text;
			}
			else if (copyText && source.Item2 != null)
			{
				commonToken.Text = source.Item2.GetText(Interval.Of(start, stop));
			}

			return commonToken;
		}

		public override CommonToken Create(int type, string text)
		{
			return new HtmlCommonToken(type, text);
		}
	}
}
