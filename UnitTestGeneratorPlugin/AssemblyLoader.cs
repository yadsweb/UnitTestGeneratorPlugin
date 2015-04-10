using System;
using System.IO;
using System.Reflection;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    /// <summary>
    /// Loads an assembly without locking the file
    /// Note: the assemblys are loaded in current domain, so they are not unloaded by this class
    /// </summary>
    public class AssemblyLoader : IDisposable
    {
        private string _assemblyLocation;
        private string _workingDirectory;
        private bool _resolveEventAssigned;

        /// <summary>
        /// Creates a copy in a new temp directory and loads the copied assembly and pdb (if existent) and the same for referenced ones. 
        /// Does not lock the given assembly nor pdb and always returns new assembly if recopiled.
        /// Note: uses Assembly.LoadFile()
        /// </summary>
        /// <returns></returns>
        public Assembly LoadFileCopy(string assemblyLocation)
        {
            lock (this)
            {
                _assemblyLocation = assemblyLocation;
                var filename = Path.GetFileName(_assemblyLocation);
                if (filename == null)
                {
                    throw new Exception("Error the file name we try to extract from path '" + _assemblyLocation + "' is null!");
                }
                if (!_resolveEventAssigned)
                {
                    _resolveEventAssigned = true;

                    AppDomain.CurrentDomain.AssemblyResolve += AssemblyFileCopyResolveEvent;
                }

                //  Create new temp directory
                _workingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(_workingDirectory);

                //  Generate copy
                var assemblyCopyPath = Path.Combine(_workingDirectory, filename);
                File.Copy(_assemblyLocation, assemblyCopyPath, true);

                //  Generate copy of referenced assembly debug info (if existent)
                string assemblyPdbPath = _assemblyLocation.Replace(".dll", ".pdb");
                if (File.Exists(assemblyPdbPath))
                {
                    string assemblyPdbCopyPath = Path.Combine(_workingDirectory, Path.GetFileName(assemblyPdbPath));
                    File.Copy(assemblyPdbPath, assemblyPdbCopyPath, true);
                }

                //  Use LoadFile and not LoadFrom. LoadFile allows to load multiple copies of the same assembly
                return Assembly.LoadFile(assemblyCopyPath);
            }
        }

        /// <summary>
        /// Creates a new copy of the assembly to resolve and loads it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyFileCopyResolveEvent(object sender, ResolveEventArgs args)
        {
            string referencedAssemblyFileNameWithoutExtension = Path.GetFileName(args.Name.Split(',')[0]);

            var directory = Path.GetDirectoryName(_assemblyLocation);
            var directoryForCopy = Path.GetDirectoryName(args.RequestingAssembly.Location);
            if (directory == null)
            {
                throw new Exception("Error the directory name we try to extract from path '" + _assemblyLocation + "' is null!");
            }
            if (directoryForCopy == null)
            {
                throw new Exception("Error the directory name for assembly copy we try to extract from path '" + Path.GetDirectoryName(args.RequestingAssembly.Location) + "' is null!");
            }

            //  Generate copy of referenced assembly
            string referencedAssemblyPath = Path.Combine(directory, referencedAssemblyFileNameWithoutExtension + ".dll");
            string referencedAssemblyCopyPath = Path.Combine(directoryForCopy, referencedAssemblyFileNameWithoutExtension + ".dll");
            File.Copy(referencedAssemblyPath, referencedAssemblyCopyPath, true);

            //  Generate copy of referenced assembly debug info (if existent)
            string referencedAssemblyPdbPath = Path.Combine(directory, referencedAssemblyFileNameWithoutExtension + ".pdb");
            if (File.Exists(referencedAssemblyPdbPath))
            {
                File.Copy(referencedAssemblyPath, referencedAssemblyCopyPath, true);
            }

            //  Use LoadFile and not LoadFrom. LoadFile allows to load multiple copies of the same assembly
            return Assembly.LoadFile(referencedAssemblyCopyPath);
        }


        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_resolveEventAssigned)
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= AssemblyFileCopyResolveEvent;

                    _resolveEventAssigned = false;
                }
            }
        }
    }
}
