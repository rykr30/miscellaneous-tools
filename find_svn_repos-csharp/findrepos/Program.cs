using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace findrepos
{
    public class SVN
    {
        public static void Run (string command, string args)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = command;
            myProcess.StartInfo.Arguments = args;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.RedirectStandardError = false;
            myProcess.StartInfo.RedirectStandardInput = false;
            myProcess.StartInfo.RedirectStandardOutput = false;

            try
            {
                myProcess.Start();
                myProcess.WaitForExit();

                if (myProcess.ExitCode == 0)
                {
                    Console.WriteLine("{0} {1} Exit code is {2}",command, args, myProcess.ExitCode);
                }
                else
                {
                    Console.WriteLine("ERROR!!! {0} {1} Exit code is {2}", command, args, myProcess.ExitCode);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!!! {0}",ex);
            }

        }

    }


    public class FindRepos
    {
        readonly List<string> _svnList = new List<string>();

        private readonly List<string> _searchList = new List<string>();

        public FindRepos(List<string> theSearchList)
        {
            _searchList = theSearchList;
        }

        public List<string> svnList
        {
            get { return _svnList; }
        }

        public void Search ()
        {
            foreach (string folder in _searchList)
            {
                ProcessDir(folder);
            }
        }


        public bool ProcessDir(string sourceDir)
        {
            // Recurse into subdirectories of this directory.
            try
            {
                string[] subdirEntries = Directory.GetDirectories(sourceDir);
                foreach (string subdir in subdirEntries)
                {
                    // Do not iterate through reparse points
                    if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                    {
                        // If we encounter a .svn folder, do not recurse any further
                        if (subdir.EndsWith(".svn"))
                        {
                            _svnList.Add(subdir.Replace(".svn",""));
                            return false;
                        }

                        bool SkipTheRest = ProcessDir(subdir);
                        if (SkipTheRest) continue;

                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Warning!  Can't access: {0}", sourceDir);
            }

            return (true);
        }
    }

    class Program
    {
        public static void PauseIfInIDE()
        {
            if (!Debugger.IsAttached) return;

            Console.WriteLine("Press <ENTER> to continue.");
            Console.Read();
        }


        public static void Usage()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage: {0} update|commit|status|info|log RepoSearchPath [RepoSeachPath...]", Path.GetFileName(Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)));
            Console.WriteLine("");
            PauseIfInIDE();
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            List<string> searchMe = new List<string>();

            if (args.Length < 2) Usage();

            string command = args[0];

            switch (command)
            {
                case "update":
                    break;

                case "commit":
                    break;

                case "status":
                    break;

                case "info":
                    break;

                case "log":
                    break;

                default:
                    Usage();
                    break;

            }


            int index = 0;
            foreach (string s in args)
            {
                if ( index > 0) searchMe.Add(s);
                index++;
            }

            FindRepos r = new FindRepos(searchMe);

            r.Search();


            foreach (string repo in r.svnList)
            {
                Console.WriteLine("====>>> BEGIN Processing: {0} ========", repo);

                SVN.Run("svn",String.Format("{0} {1}",command,repo));

                Console.WriteLine("====<<< END Processing: {0} ========", repo);
                Console.WriteLine("");
            }

            PauseIfInIDE();
        }
    }
}
