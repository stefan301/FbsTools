namespace Fbs2Html
{
	internal class TypeDeclaration
	{
		internal enum Kind
		{
			Enum,
			Union,
			Struct,
			Table
		}
		public TypeDeclaration(Kind kind, string namespaceName, string name, string fileName)
		{
			Type = kind;
			Namespace = namespaceName;
			Name = name;
			FullyQualifiedName = string.IsNullOrEmpty(namespaceName) ? name : namespaceName + "." + name;
			FileName = fileName;
		}
		public string Namespace { get; private set; }
		public string Name { get; private set; }
		public string FullyQualifiedName { get; private set; }
		public string FileName { get; private set; }
		public Kind Type { get; set; }
		public bool IsRoot { get; set; } = false;
	}
}
