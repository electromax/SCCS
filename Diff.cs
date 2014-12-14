using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sccs
{
    /// <summary>
    /// Solves the problem of creating changeset.
    /// </summary>
    class Diff
    {
        /// <summary>
        /// List of lines of the original text
        /// </summary>
        List<string> originalText = new List<string>();

        /// <summary>
        /// List of lines of the new text
        /// </summary>
        List<string> newText = new List<string>();

        /// <summary>
        /// Operation done in the dynamic programming
        /// </summary>
        char[,] operation;

        /// <summary>
        /// List of replacements made.
        /// </summary>
        List<Replacement> madeReplacements = new List<Replacement>();
        
        /// <summary>
        /// Lines count of the original text
        /// </summary>
        int m;

        /// <summary>
        /// Lines count of the new text
        /// </summary>
        int n;
        
        /// <summary>
        /// Where the difference of texts begins
        /// </summary>
        int posBegin;
        
        /// <summary>
        /// Inputs data from files
        /// </summary>
        void Input(string originalFileName, string newFileName)
        {
            string line;
            
            using (StreamReader originalFile = new StreamReader(originalFileName, Encoding.Default))
                while ((line = originalFile.ReadLine()) != null)
                    originalText.Add(line);
            
            using (StreamReader newFile = new StreamReader(newFileName, Encoding.Default))
                while ((line = newFile.ReadLine()) != null)
                    newText.Add(line);
           
            m = originalText.Count;
            n = newText.Count;
        }
        
        /// <summary>
        /// Skips the first lines that are equal in both texts
        /// </summary>
        void SkipFirstLines()
        {
            posBegin = 0;
            while (posBegin < m && posBegin < n && originalText[posBegin] == newText[posBegin])
                posBegin++;
        }
        
        /// <summary>
        /// Calculates, what lines should be added, removed and replaced 
        /// in the original text to get the new one, using the method of
        /// "dynamic programming" (calculates the minimal edit distance).
        /// For more information see http://en.wikipedia.org/wiki/Dynamic_programming
        /// and http://www.csse.monash.edu.au/~lloyd/tildeAlgDS/Dynamic/Edit/
        /// The result of this procedure is the filled matrix
        /// (m - posBegin)*(n - posBegin) with the operations.
        /// </summary>
        void DynamicProgramming()
        {
            // original text in matrices lasts for rows,
            // and new text lasts for columns

            // we won't store the whole matrix of the minimum-edit-distances,
            // it is sufficiently to store the two rows
            
            // represents the row in matrix:
            ushort[] vec1 = new ushort[n - posBegin + 1];

            // represents the next row in matrix:
            ushort[] vec2 = new ushort[n - posBegin + 1];

            // the matrix of operations, vice-versa, is whole,
            // and contains such operations:
            // "A" : the last line in the row needs to be added to original text
            // "D" : the last line in the column needs to be deleted from original text
            // "R" : the last line in the row (in original text) needs to be
            //       replaced with the last linein the column
            // "E" : two last lines are equal
            
            operation = new char[m - posBegin + 1, n - posBegin + 1];

            // fill the first row:
            int i = 0, j;
            for (j = 0; posBegin + j <= n; j++)
            {
                vec1[j] = (ushort)j;
                operation[i, j] = 'A';
            }
            operation[0, 0] = 'E';

            if (posBegin == m) // thare are no rows (no original text after posBegin)
                return;

            // fill other rows, each row with the help of the previous row:
            for (i++; ; i++)
            {
                // fill the first column in this row:
                j = 0;
                vec2[j] = (ushort)(vec1[j] + 1);
                operation[i, j] = 'D';

                // fill the other columns, each column with the help from previous:
                for (j++; posBegin + j <= n; j++)
                {
                    // try to delete:
                    ushort distanceIfDelete = (ushort)(vec1[j] + 1);

                    // try to add:
                    ushort distanceIfAdd = (ushort)(vec2[j - 1] + 1);

                    // try to find equals:
                    ushort distanceIfEqual = ushort.MaxValue;
                    if (originalText[posBegin + i - 1] == newText[posBegin + j - 1])
                        distanceIfEqual = vec1[j - 1];

                    // try to replace:
                    ushort distanceIfReplace = (ushort)(vec1[j - 1] + 1);

                    // choose the minimum distance from this 4 alternatives:
                    ushort min = distanceIfDelete;
                    if (distanceIfAdd < min)
                        min = distanceIfAdd;
                    if (distanceIfEqual < min)
                        min = distanceIfEqual;
                    if (distanceIfReplace < min)
                        min = distanceIfReplace;

                    // set the distance:
                    vec2[j] = min;

                    // set the right operation:
                    if (min == distanceIfDelete)
                        operation[i, j] = 'D';
                    else if (min == distanceIfEqual)
                        operation[i, j] = 'E';
                    else if (min == distanceIfAdd)
                        operation[i, j] = 'A';
                    else
                        operation[i, j] = 'R';
                }
                if (posBegin + i == m)
                    break;

                // for the next iteration, the produced "new" row
                // becomes the "previous" row
                for (j = 0; posBegin + j <= n; j++)
                    vec1[j] = vec2[j];
            }
        }
        
        /// <summary>
        /// Makes the approximation of replacements list, using the matrix,
        /// calculated by the dynamic programming method.
        /// This approximation is not obliged to be non-ambiguous.
        /// </summary>
        void MakeReplacementsList()
        {
            // Analyze the matrix of operations.
            // Start with the last element (the operation for the whole lines).
            int i = m - posBegin;
            int j = n - posBegin;
            Replacement r = new Replacement();
            while (i >= 0 || j >= 0)
                switch (operation[i, j])
                {
                    case 'D': // delete line # "posBegin + i - 1"
                        i--;
                        if (r.FindRegion == null) // not initialized
                            r.FindRegion = new Region(posBegin + i, posBegin + i + 1);
                        else // if already deleted something, just move backward:
                            r.FindRegion.Begin--;
                        if (r.ReplaceRegion == null) // mark the position in the new text
                            r.ReplaceRegion = new Region(posBegin + j, posBegin + j);
                        break;
                    case 'A': // add line # "posBegin + j - 1"
                        j--;
                        if (r.ReplaceRegion == null) // not initialized
                            r.ReplaceRegion = new Region(posBegin + j, posBegin + j + 1);
                        else // if already added something, just move backward:
                            r.ReplaceRegion.Begin--;
                        if (r.FindRegion == null) // mark the position in the original text
                            r.FindRegion = new Region(posBegin + i, posBegin + i);
                        break;
                    case 'E': // line are equals
                        i--;
                        j--;
                        // if some replacement was initialized, save it and start a new:
                        if (r.FindRegion != null || r.ReplaceRegion != null)
                        {
                            madeReplacements.Add(r);
                            r = new Replacement();
                        }
                        break;
                    case 'R': // replace line # "posBegin + i - 1" with line # "posBegin + j - 1"
                        i--;
                        if (r.FindRegion == null) // not initialized
                            r.FindRegion = new Region(posBegin + i, posBegin + i + 1);
                        else // if already deleted something, just move backward:
                            r.FindRegion.Begin--;
                        j--;
                        if (r.ReplaceRegion == null) // not initialized
                            r.ReplaceRegion = new Region(posBegin + j, posBegin + j + 1);
                        else // if already added something, just move backward:
                            r.ReplaceRegion.Begin--;
                        break;
                }
        }

        /// <summary>
        /// Expands the regions in the replacement list so that all
        /// replacements become non-ambiguous.
        /// When the regions expand so that they overlap each other,
        /// they are united in this procedure.
        /// </summary>
        void ExpandReplaceList()
        {
            for (int ri = 0; ri < madeReplacements.Count; ri++)
            {
                Replacement r = madeReplacements[ri];
                Region find = r.FindRegion; // "Find" part of a replacement
                
                // analog is the region that can be the same that "Find" and thus
                // provide the ambiguity. We'll find such analogs and fix.
                Region analog = new Region(0, find.End - find.Begin);
                
                // save initial values of "Find":
                int savedBegin = find.Begin;
                int savedEnd = find.End;
                
                // test all analogs till the end:
                while (analog.End <= m)
                {
                    if (analog.Begin != find.Begin && analog.Same(find, originalText))
                    {
                        // ambiguity is found.

                        // try to expand the "Find" region so that it is not the same as the analog:
                        int passForward2, passBackward2;
                        GetPassBoth(find, analog, out passForward2, out passBackward2);
                        int passForward = GetPassForward(find, analog, passForward2 + passBackward2);
                        int passBackward = GetPassBackward(find, analog, passForward2 + passBackward2);
                        
                        // while tried 3 methods of expanding (see above), choose the minimal:
                        if (passForward < passBackward)
                        {
                            // expand forward
                            find.End += passForward;
                            analog.End += passForward;
                        }
                        else if (passForward >= passBackward && passBackward < passForward2 + passBackward2)
                        {
                            // expand backward
                            find.Begin -= passBackward;
                            analog.Begin -= passBackward;
                        }
                        else
                        {
                            // expand forward and backward
                            find.End += passForward2;
                            analog.End += passForward2;
                            find.Begin -= passBackward2;
                            analog.Begin -= passBackward2;
                        }
                    }
                    // continue to find ambiguities, using expanded "Find" and analog:
                    analog.Begin++;
                    analog.End++;
                } // while the cycle is over, all ambiguities with this "Find" are fixed.

                // if "Find" is expanded, "Replace" should be expanded also:
                r.ReplaceRegion.Begin += find.Begin - savedBegin;
                r.ReplaceRegion.End += find.End - savedEnd;

                // test for overlapping with the previous replacements
                // (the previous replacements have always bigger Begins and Ends):
                while (ri != 0)
                {
                    Replacement previous = madeReplacements[ri - 1];
                    Region previousFind = previous.FindRegion;
                    if (previousFind.Begin < find.End)   // overlaps
                    {
                        r.ReplaceRegion.End += previousFind.Begin - find.End +
                            previous.ReplaceRegion.End -
                            previous.ReplaceRegion.Begin; // fix the end of "Replace"
                        if (find.End < previousFind.End)
                            find.End = previousFind.End; // if not nested, unite
                        else // if nested, change the "Replace" end:
                            r.ReplaceRegion.End += find.End - previousFind.End;
                        ri--;
                        madeReplacements.RemoveAt(ri);
                        continue;
                    }
                    break;
                }

                // test for overlapping with the next replacements
                // (the next replacements have always smaller Begins and Ends):
                while (ri + 1 != madeReplacements.Count)
                {
                    Replacement next = madeReplacements[ri + 1];
                    Region nextFind = next.FindRegion;
                    if (nextFind.End > find.Begin)   // overlaps
                    {
                        r.ReplaceRegion.Begin -= find.Begin - nextFind.End + 
                            next.ReplaceRegion.End -
                            next.ReplaceRegion.Begin;  // fix the begin of "Replace"
                        if (find.Begin > nextFind.Begin)
                            find.Begin = nextFind.Begin; // if not nested, unite
                        else // if nested, change the "Replace" begin:
                            r.ReplaceRegion.Begin -= nextFind.Begin - find.Begin;
                        madeReplacements.RemoveAt(ri + 1);
                        continue;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Looks for expanding a region so that it could not be the same as another.
        /// Makes passes forward and backward.
        /// </summary>
        /// <param name="find">The main region</param>
        /// <param name="analog">The region that seems the same</param>
        /// <param name="passForward">How many lines were analyzed forward</param>
        /// <param name="passBackward">How many lines were analyzed backward</param>
        void GetPassBoth(Region find, Region analog, out int passForward,
            out int passBackward)
        {
            // save the initial values:
            int analogBegin = analog.Begin;
            int analogEnd = analog.End;
            int findBegin = find.Begin;
            int findEnd = find.End;

            while (true)
            {
                if (analogBegin > 0 && findBegin > 0 && originalText[analogBegin - 1] != originalText[findBegin - 1])
                { // can move backward:
                    findBegin--;
                    analogBegin--;
                }
                else if (analogBegin == 0 && findBegin > 0)
                { // can move backward (and analog goes over the bounds):
                    findBegin--;
                    analogBegin--;
                }
                else if (analogEnd < m && findEnd < m && originalText[analogEnd] != originalText[findEnd])
                { // can move forward:
                    findEnd++;
                    analogEnd++;
                }
                else if (analogEnd == m && findEnd < m)
                { // can move forward (and analog goes over the bounds):
                    findEnd++;
                    analogEnd++;
                }
                else
                { // forward and backward symbols are the same
                  // --> move and continue the cycle
                    if (findBegin > 0)
                    {
                        findBegin--;
                        analogBegin--;
                    }
                    if (findEnd < m)
                    {
                        findEnd++;
                        analogEnd++;
                    }
                    continue;
                }
                break;
            }

            // calculates the passes:
            passBackward = find.Begin - findBegin;
            passForward = findEnd - find.End;
        }

        /// <summary>
        /// Looks for expanding a region so that it could not be the same as another.
        /// Makes passes only forward.
        /// Stops when passed more than a given limit.
        /// </summary>
        /// <param name="find">The main region</param>
        /// <param name="analog">The region that seems the same</param>
        /// <param name="Limit">The limit</param>
        /// <returns>How many lines were analyzed</returns>
        int GetPassForward(Region find, Region analog, int Limit)
        {
            int i;
            for (i = 0; i < Limit; i++)
            {
                if ((analog.End + i < m && find.End + i < m && originalText[analog.End + i]
                    != originalText[find.End + i]) || (analog.End + i == m && find.End + i < m))
                    // a difference found
                    break;
                if (find.End + i == m)
                {
                    // end of text exceeded:
                    i = Limit;
                    break;
                }
            }
            if (i == Limit)
                // limit exceeded:
                i = int.MaxValue / 4;
            return i + 1;
        }

        /// <summary>
        /// Looks for expanding a region so that it could not be the same as another.
        /// Makes passes only backward.
        /// Stops when passed more than a given limit.
        /// </summary>
        /// <param name="find">The main region</param>
        /// <param name="analog">The region that seems the same</param>
        /// <param name="Limit">The limit</param>
        /// <returns>How many lines were analyzed</returns>
        int GetPassBackward(Region find, Region analog, int Limit)
        {
            int i;
            for (i = 0; i < Limit; i++)
            {
                if ((analog.Begin - i > 0 && find.Begin - i > 0 && originalText[analog.Begin - i - 1]
                    != originalText[find.Begin - i - 1]) || (analog.Begin - i <= 0 && find.Begin - i > 0))
                    // a difference found
                    break;
                if (find.Begin - i <= 0)
                {
                    // begin of text exceeded:
                    i = Limit;
                    break;
                }
            }
            if (i == Limit)
                // limit exceeded:
                i = int.MaxValue / 4;
            return i + 1;
        }

        /// <summary>
        /// Outputs the obtained changeset into a file
        /// </summary>
        /// <param name="changeSetFileName"></param>
        void Output(string changeSetFileName)
        {
            int i, j;
            using (StreamWriter output = new StreamWriter(changeSetFileName, false, 
                Encoding.Default))
            {
                output.WriteLine("Changeset");
                output.WriteLine();
                for (i = madeReplacements.Count - 1; i >= 0; i--)
                {
                    Replacement r = madeReplacements[i];
                    output.WriteLine("Find:");
                    for (j = r.FindRegion.Begin; j < r.FindRegion.End; j++)
                        output.WriteLine(">" + originalText[j]);
                    output.WriteLine();
                    output.WriteLine("Replace:");
                    for (j = r.ReplaceRegion.Begin; j < r.ReplaceRegion.End; j++)
                        output.WriteLine(">" + newText[j]);
                    output.WriteLine();
                }
            }
        }

        /// <summary>
        /// Entry point for this class; does all the work.
        /// </summary>
        public void Main(string originalFileName, string newFileName, 
            string changeSetFileName)
        {
            Input(originalFileName, newFileName);
            SkipFirstLines();
            DynamicProgramming();
            MakeReplacementsList();
            ExpandReplaceList();
            Output(changeSetFileName);
        }
    }
}
