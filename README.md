SCCS
====

Problem: Source Code Change Sets

A console program that can perform the following operations:

•	generate a change set file C(A,B) for any given pair of text files (A, B)

•	apply a given change set file C(A,B) to an input file X and produce the target file Y.

Detailed specification:

•	Files A and B are plain ANSI charset text files (ordered collection of lines delimited by ‘\n’ character or ‘\r\n’ character combination).

•	The change set file C(A,B) should contain human readable instructions to convert file A into file B.

•	The conversion instructions should work with whole lines (do not refer to words or symbols inside a line). Lines that differ at least in one character should be considered different (treat differences in whitespace as significant).

•	The conversion instructions must be bound to the surrounding lines context (SLC).

• When applied to the input file A the change set file C(A,B) must produce the original file B

•	The program must allow applying the change set file to any text file if the contents of the text file are compatible with the contexts found in the change set file. Otherwise the error description should be printed out to the console. The two basic types of errors are: “required context not found” and “change set applying is ambiguous”.

