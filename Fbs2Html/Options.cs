using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fbs2Html
{
	internal class Options
	{
		[Option('f', "files", Required = true, HelpText = "Files (filenames oder directories) for .html file generation")]
		public IEnumerable<string> Paths { get; set; }

		[Option('o', "output", Required = true, HelpText = "Output folder for the .html files")]
		public string OutputFolder { get; set; }

		public static string GetUsage<T>(ParserResult<T> result)
		{
			return HelpText.AutoBuild(result, Parser.Default.Settings.MaximumDisplayWidth);
		}
	}
}
