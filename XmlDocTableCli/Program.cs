using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace XmlDocTableCli
{
    /// <summary>The command line interface.</summary>
    class Program
    {
        /// <summary>The builder of the output string.</summary>
        static StringBuilder sb = new StringBuilder();

        /// <summary>Outputs a LaTeX header</summary>
        static void Header(int n, string s) => sb.AppendLine(@"\hiderowcolors \multicolumn{" + n + @"}{l}{" + s + @"} \tabularnewline \midrule \showrowcolors");

        /// <summary>The main method.</summary>
        static void Main(string[] args)
        {
            var walker = new TexTableWalker();
            foreach (var src in args.SelectMany(arg => Directory.EnumerateFiles(arg, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText)))
                walker.Visit(CSharpSyntaxTree.ParseText(src).GetRoot());
            sb.AppendLine(@"% AUTOGENERATED");
            sb.AppendLine(@"\begin{longtabu} to \textwidth { l | X }");
            Header(2, "Classes");
            sb.Append(walker.ClassTable);
            sb.AppendLine(@"\bottomrule");
            sb.AppendLine(@"\end{longtabu}");
            foreach (var table in walker.MemberTables)
            {
                sb.AppendLine($"\n{table.Key}\n");
                sb.AppendLine(@"\begin{longtabu} to \textwidth { l | X | X | X[2] | X[3] }");
                if (table.Value.FieldsCount > 0)
                {
                    Header(5, "Fields");
                    sb.AppendLine(@"\textbf{Name} & \textbf{Modifiers} & \textbf{Type} & \multicolumn{2}{l}{\textbf{Description}} \\");
                    sb.Append(table.Value.FieldsTable);
                }
                if (table.Value.PropertiesCount > 0)
                {
                    Header(5, "Properties");
                    sb.AppendLine(@"\textbf{Name} & \textbf{Modifiers} & \textbf{Type} & \textbf{Accessors} & \textbf{Description} \\");
                    sb.Append(table.Value.PropertiesTable);
                }
                if (table.Value.MethodsCount > 0)
                {
                    Header(5, "Methods");
                    sb.AppendLine(@"\textbf{Name} & \textbf{Modifiers} & \textbf{Return Type} & \textbf{Parameters} & \textbf{Description} \\");
                    sb.Append(table.Value.MethodsTable);
                }
                sb.AppendLine(@"\bottomrule");
                sb.AppendLine(@"\end{longtabu}");
            }
            sb.AppendLine(@"% /AUTOGENERATED");
            Console.Write(sb);
        }
    }
}
