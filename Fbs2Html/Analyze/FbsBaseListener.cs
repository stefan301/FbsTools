using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using FbsParser;
using System.Linq;
using System.Text;

namespace Fbs2Html
{
	internal class FbsBaseListener : FlatBuffersBaseListener
	{
		public string CurrentFilename { get; internal set; }
		public BufferedTokenStream Tokens { get; internal set; }
		protected string CurrentNamespace { get; set; }

		public override void EnterNamespace_decl([NotNull] FlatBuffersParser.Namespace_declContext context)
		{
			CurrentNamespace = BuildNamespaceName(context);
		}

		private string BuildNamespaceName([NotNull] FlatBuffersParser.Namespace_declContext context)
		{
			var sb = new StringBuilder();


			var identifier = context.identifier();
			var dots = context.DOT();

			for (int i = 0; i < identifier.Length; ++i)
			{
				var ident = identifier[i].IDENT().Payload as CommonToken;
				sb.Append(ident.Text);
				if (i < dots.Length)
					sb.Append('.');
			}

			return sb.ToString();
		}

		protected string BuildFullyQualifiedIdentifier([NotNull] FlatBuffersParser.Ns_identContext context)
		{
			var identifier = context.identifier();
			var dots = context.DOT();

			var sb = new StringBuilder();

			if ( !dots.Any() )
			{
				sb.Append(CurrentNamespace);
				if (!string.IsNullOrEmpty(CurrentNamespace))
					sb.Append(".");
			}

			for (int i = 0; i < identifier.Length; ++i)
			{
				var ident = identifier[i].IDENT().Payload as CommonToken;
				sb.Append(ident.Text);
				if (i < dots.Length)
					sb.Append('.');
			}

			return sb.ToString();
		}
	}
}
