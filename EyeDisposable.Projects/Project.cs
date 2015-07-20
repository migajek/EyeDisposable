using EyeDisposable.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EyeDisposable.Projects
{
    public class Project
    {
        public string Name { get; set; }
        public List<ProjectFile> Files { get; set; }
        public string BaseDir { get; set; }

        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(this));
        }

        public static Project LoadFromFile(string fileName)
        {
            return JsonConvert.DeserializeObject<Project>(File.ReadAllText(fileName));
        }

        public Project()
        {
            Files = new List<ProjectFile>();
        }

        public Project(string name, string baseDir):this()
        {
            Name = name;
            BaseDir = baseDir;
        }

        public void AddFile(string fileName)
        {
            if (Path.GetFileName(fileName) != fileName)
                fileName = fileName.MakeRelativePath(BaseDir);
            Files.Add(new ProjectFile()
            {
                FileName = fileName
            });
        }

        public void Instrument()
        {
            var filesToCopy = new[] { "EyeDisposable.Logger.dll", "EyeDisposable.Logger.pdb" };

            foreach (var file in Files)
            {
                var fileName = Path.Combine(BaseDir, file.FileName);
                var targetDir = Path.GetDirectoryName(fileName);
                try {
                    Console.WriteLine("Instrumenting {0}", fileName);
                    new Instrumenter(targetDir).Instrument(fileName, fileName);
                } catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var directory in Files.Select(x => Path.GetDirectoryName(x.GetAbsoluteFileName(BaseDir))).Distinct())
                foreach (var file in filesToCopy)
                    File.Copy(Path.Combine(exeDir, file), Path.Combine(directory, file), true);
        }
    }

    public class ProjectFile
    {
        public string FileName { get; set; }
        public string GetAbsoluteFileName(string baseDir)
        {
            return Path.Combine(baseDir, FileName);
        }
    }
}
