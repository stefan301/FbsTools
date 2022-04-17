using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CommandLine;
using CommandLine.Text;
using FbsParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fbs2Html
{
	internal class Program
	{
                static int Main(string[] args)
                {
                        var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
                        var resultRun = result.MapResult(Run, _ => {
                                return false;
                        }) ? 0 : 1;

                        if (resultRun == 1)
                        {
                                var helpText = HelpText.AutoBuild(result, h => h, e =>
                                {
                                        return e;
                                });
                                Console.WriteLine(helpText);
                        }

                        return resultRun;
                }

                private static HashSet<string> CollectFiles(Options options)
		{
                        var files = new HashSet<string>();

                        Action<string> collectFiles = (path) =>
                        {
                                if (Directory.Exists(path))
                                {
                                        foreach (var item in Directory.GetFiles(path, "*.fbs", SearchOption.AllDirectories))
                                                files.Add(item);
                                }
                                else if (File.Exists(path) && Path.GetExtension(path) == ".fbs")
                                {
                                        files.Add(path);
                                }
                        };

			foreach (var item in options.Paths)
			{
                                collectFiles(item);
                        }

                        return files;
                }

                private static bool Run(Options options)
                {
                        var inputFiles = CollectFiles(options);
                        if( !inputFiles.Any())
                                return false;

                        var output = Console.Out;
                        var errorOutput = Console.Error;

                        string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        string exeDirectory = Path.GetDirectoryName(exeFilePath);
                        string htmlDirectory = Path.Combine(exeDirectory, "html");

                        string inputRootFolder = Directory.GetCurrentDirectory();

                        var symbolTable = new SymbolTable();
                        var symbolTableListener = new SymbolTableListener(symbolTable, errorOutput);
                        var htmlGenerator = new HtmlGenerator(symbolTable, errorOutput, inputRootFolder, (fileName, content) =>
                        {
                                try
                                {
                                        fileName = Path.Combine(options.OutputFolder, fileName);

                                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                                        File.WriteAllText(fileName, content);
                                }
                                catch (Exception ex)
                                { Console.WriteLine(ex.ToString()); }

                        });

                        ParseAll(inputFiles, symbolTableListener, output, errorOutput);
                        ParseAll(inputFiles, htmlGenerator, output, errorOutput);

                        htmlGenerator.WriteIndex();

                        Action<string> CopyFileFromHtmlFolderToOutputFolder = (fileName) =>
                        {
                                string sourceFilePath = Path.Combine(htmlDirectory, fileName);
                                string destinationFilePath = Path.Combine(options.OutputFolder, fileName);

                                try
                                {
                                        File.Copy(sourceFilePath, destinationFilePath, true);
                                }
                                catch (Exception ex)
                                {
                                        Console.WriteLine($"Failed to copy {sourceFilePath} to {destinationFilePath}: {ex.Message}");
                                }
                        };

                        CopyFileFromHtmlFolderToOutputFolder("style.css");
                        CopyFileFromHtmlFolderToOutputFolder("tree.js");
                        CopyFileFromHtmlFolderToOutputFolder("stickyHeader.js");

                        return true;
                }

                private static void ParseAll(IEnumerable<string> paths, FbsBaseListener listener, TextWriter output, TextWriter errorOutput)
                {
                        foreach (var path in paths)
                        {
                                ParseFbs(path, listener, output, errorOutput);
                        }
                }
                private static void ParseFbs(string path, FbsBaseListener listener, TextWriter output, TextWriter errorOutput)
                {
                        var encoding = GetEncoding(path);
                        var stream = CharStreams.fromPath(path, encoding);
                        var tokenFacory = new HtmlCommonTokenFactory();
                        
                        var lexer = new FlatBuffersLexer(stream, output, errorOutput);
                        lexer.TokenFactory = tokenFacory;

                        var tokens = new CommonTokenStream(lexer);
                        var parser = new FlatBuffersParser(tokens, output, errorOutput);

                        RuleContext tree = parser.schema();

                        listener.CurrentFilename = Path.GetFullPath( path );
                        listener.Tokens = tokens;

                        var walker = new ParseTreeWalker();
                        walker.Walk(listener, tree);
                }

                /// <summary>
                /// Determines a text file's encoding by analyzing its byte order mark (BOM).
                /// Defaults to ASCII when detection of the text file's endianness fails.
                /// </summary>
                /// <param name="filename">The text file to analyze.</param>
                /// <returns>The detected encoding.</returns>
                public static Encoding GetEncoding(string filename)
                {
                        // Read the BOM
                        var bom = new byte[4];
                        using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                        {
                                file.Read(bom, 0, 4);
                        }

                        // Analyze the BOM
                        if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
                        if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
                        if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
                        if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
                        if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
                        if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

                        // We actually have no idea what the encoding is if we reach this point, so
                        // you may wish to return null instead of defaulting to ASCII
                        //            return Encoding.ASCII;
                        return Encoding.GetEncoding(1252);
                }
        }
}
