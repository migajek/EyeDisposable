using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using EyeDisposable.Core;
using System.Reflection;
using EyeDisposable.Projects;

namespace EyeDisposable
{
    static class Program
    {
        static void Usage()
        {
            var selfName = Path.GetFileName(Assembly.GetEntryAssembly().Location);

            Console.WriteLine("EyeDisposable by Chris Yuen <chris@kizzx2.com> 2011");
            Console.WriteLine();
            Console.WriteLine("Instrument assembly to catch IDispose leaks.");
            Console.WriteLine();
            Console.WriteLine("Example: {0} foo.exe", selfName);
            Console.WriteLine("Example: {0} foo.dll", selfName);
            Console.WriteLine("Working with project: run {0} project", selfName);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            if (args.First() == "project")
            {
                if (args.Length == 1)
                    HandleProject();
                else
                {
                    var proj = Projects.Project.LoadFromFile(args.Skip(1).First());
                    Console.WriteLine("Project loaded, instrumenting ...");
                    proj.Instrument();
                    Console.WriteLine("done");
                }
            }
            else
            {
                var targetDir = Path.GetDirectoryName(args[0]);

                new Instrumenter(targetDir).Instrument(args[0], args[0]);

                // Put EyeDisposable.Logger.dll next to my target
                File.Copy("EyeDisposable.Logger.dll", Path.Combine(
                    targetDir, "EyeDisposable.Logger.dll"), true);

                if (File.Exists("EyeDisposable.Logger.pdb"))
                    File.Copy("EyeDisposable.Logger.pdb", Path.Combine(targetDir,
                        "EyeDisposable.Logger.pdb"), true);
            }
        }

        private static void HandleProject()
        {
            var info = new[]
            {
                "available commands: ",
                "new <name> <basedir>",
                "add <filename>",
                "save",
                "instrument",
                "q - quit"
            };
            string cmd = "";
            Projects.Project current = null;
            while (cmd != "q")
            {
                foreach (var i in info)
                    Console.WriteLine(" # " + i);
                Console.WriteLine(" # {0}", current != null ? "current project: " + current.Name : "no project loaded");

                cmd = Console.ReadLine().Trim();
                var tokens = cmd.SplitArguments();
                try {
                    switch (tokens.First())
                    {
                        case "new":
                            current = new Projects.Project(tokens[1], tokens[2]);
                            break;
                        case "add":
                            if (current == null)
                                Console.WriteLine("No project loaded");
                            else
                                current.AddFile(tokens[1]);
                            break;
                        case "save":
                            if (current == null)
                                Console.WriteLine("No project loaded");
                            else
                                current.SaveToFile(Path.Combine(current.BaseDir, "eyedisposable.proj"));
                            break;
                        case "instrument":
                            current.Instrument();
                            break;
                        case "load":
                            current = Project.LoadFromFile(tokens[1]);
                            break;
                    }
                } catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
