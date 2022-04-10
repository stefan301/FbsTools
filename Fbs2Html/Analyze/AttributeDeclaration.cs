namespace Fbs2Html
{
	internal class AttributeDeclaration
	{
		public AttributeDeclaration(string name, string fileName)
		{
			Name = name;
			FileName = fileName;
		}
		public string Name { get; private set; }
		public string FileName { get; private set; }
	}
}
