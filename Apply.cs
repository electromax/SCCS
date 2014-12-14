using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sccs
{
    /// <summary>
    /// Solves the problem of applying changes.
    /// </summary>
    class Apply
    {
        /// <summary>
        /// List of lines of the original text
        /// </summary>
        List<string> originalText = new List<string>();

        /// <summary>
        /// Inputs data from a file
        /// </summary>
        void Input(string originalFileName)
        {
            string line;
            using (StreamReader input = new StreamReader(originalFileName, Encoding.Default))
                while ((line = input.ReadLine()) != null)
                    originalText.Add(line);
        }

        /// <summary>
        /// Searches the pattern (subset of lines) in the long text (container).
        /// </summary>
        /// <param name="container">Where to search</param>
        /// <param name="pattern">What to search</param>
        /// <returns>The index of occurrence</returns>
        int Search(List<string> container, List<string> pattern)
        {
            int result = -1;  // not found
            for (int i = 0; i + pattern.Count <= container.Count; i++)
            {
                // compare the pattern and the sub-set beginning at i-th position:
                bool ok = true;
                for (int j = 0; j < pattern.Count; j++)
                    if (container[i + j] != pattern[j])
                    {
                        ok = false;
                        break;
                    }
                if (ok)
                {
                    // found the required subset
                    if (result < 0)
                        // it is the first occurrence
                        result = i;
                    else
                        // it is the second occurrence --> ambiguous
                        return -2;
                }
            }
            return result;
        }
        
        /// <summary>
        /// Solves the task of applying changeset to list of lines
        /// </summary>
        /// <returns>New (changed) list of lines</returns>
        List<string> Solve(string changesetFileName)
        {
            int i = 0, m = originalText.Count;
            string line;

            // the strings to write in the output file:
            List<string> result = new List<string>();
            
            // read the changeset:
            using (StreamReader cs = new StreamReader(changesetFileName, Encoding.Default))
            {
                while (true)
                {
                    line = cs.ReadLine();
                    if (line == null || line == "Find:")
                        break;
                }
                while (line == "Find:")
                {
                    // read the "Find:" clause:
                    List<string> Find = new List<string>();
                    line = cs.ReadLine();
                    while (line != "Replace:")
                    {
                        if (line.StartsWith(">"))
                            Find.Add(line.Remove(0, 1));
                        line = cs.ReadLine();
                    }

                    // read the "Replace:" clause:
                    List<string> Replace = new List<string>();
                    line = cs.ReadLine();
                    while (line != null && line != "Find:")
                    {
                        if (line.StartsWith(">"))
                            Replace.Add(line.Remove(0, 1));
                        line = cs.ReadLine();
                    }

                    // search for the "Find:" pattern:
                    int idx = Search(originalText, Find);

                    if (idx == -1) // error (code -1)
                    {
                        result.Clear();
                        result.Add("required context not found");
                        Console.WriteLine("required context not found");
                        return result;
                    }

                    if (idx == -2) // error (code -2)
                    {
                        result.Clear();
                        result.Add("change set applying is ambiguous");
                        Console.WriteLine("change set applying is ambiguous");
                        return result;
                    }
                    
                    // write the lines before the index of pattern:
                    for (; i < idx; i++)
                        result.Add(originalText[i]);

                    // write the lines from "Replace:" clause:
                    result.AddRange(Replace);

                    i += Find.Count;
                }
            }

            // write the rest of lines:
            for (; i < originalText.Count; i++)
                result.Add(originalText[i]);

            return result;
        }

        /// <summary>
        /// Entry point for this class; does all the work.
        /// </summary>
        public void Main(string originalFileName, string outputFileName, string changesetFileName)
        {
            Input(originalFileName);
            using (StreamWriter output = new StreamWriter(outputFileName, false, 
                Encoding.Default))
                foreach (string s in Solve(changesetFileName))
                    output.WriteLine(s);
        }
    }
}
