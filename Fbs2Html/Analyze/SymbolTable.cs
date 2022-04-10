using System.Collections.Generic;

namespace Fbs2Html
{
	internal class SymbolTable
	{
		public Dictionary<string, TypeDeclaration> TypeDeclarations { get; } = new Dictionary<string, TypeDeclaration>();
		public Dictionary<string, AttributeDeclaration> AttributeDeclarations { get; } = new Dictionary<string, AttributeDeclaration>();
		public Dictionary<string, string> Files { get; } = new Dictionary<string, string>();
		public ISet<string> Namespaces { get; } = new SortedSet<string>();
	}
}
