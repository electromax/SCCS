using System;

namespace Sccs
{
    /// <summary>
    /// Main class of program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        static void Main(string[] Args)
        {
            if (Args.Length == 3)
                new Diff().Main(Args[0], Args[1], Args[2]);
            else if (Args.Length == 4 && Args[3] == "/apply")
                new Apply().Main(Args[0], Args[1], Args[2]);
            else // message about usage
                Console.WriteLine(@"Usage:

sccs.exe input_file_1 input_file_2 changeset_file
  Analyze input files input_file_1 and input_file_2, generate instructions
  to convert input_file_1 to input_file_2, and output the conversion
  instructions into the changeset_file.

sccs.exe input_file output_file changeset_file /apply
  Apply the changeset_file to the input_file and output the results
  to the output_file.");
        }
     }
}
