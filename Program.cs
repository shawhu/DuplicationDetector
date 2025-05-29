using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuplicationDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DuplicationDetector (v0.8.0)");
            Console.WriteLine("Author:\tHarry Xiao");
            Console.WriteLine("Email:\tshawhu@gmail.com");
            Console.WriteLine("");
            Console.WriteLine("");


            // Parse flags
            bool targetRecursive = false;
            bool sourceRecursive = false;

            List<string> argList = args.ToList();
            // Detect and remove flags from argList
            for (int i = 0; i < argList.Count;)
            {
                if (argList[i].Equals("-tr", StringComparison.OrdinalIgnoreCase))
                {
                    targetRecursive = true;
                    argList.RemoveAt(i);
                }
                else if (argList[i].Equals("-sr", StringComparison.OrdinalIgnoreCase))
                {
                    sourceRecursive = true;
                    argList.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (argList.Count < 2)
            {
                Console.WriteLine("ERROR: Please provide a source folder and at least one target folder.");
                Console.WriteLine("Usage: DuplicationDetector [-tr] [-sr] <source_folder> <target_folder1> [target_folder2] ...");
                Console.WriteLine("Options:");
                Console.WriteLine("  -tr     Recursively check target folders");
                Console.WriteLine("  -sr     Recursively check source folder");
                return;
            }

            string sourceFolder = argList[0];
            if (!Directory.Exists(sourceFolder))
            {
                Console.WriteLine($"ERROR: Source folder '{sourceFolder}' does not exist.");
                return;
            }

            //write executing info:
            Console.WriteLine($"Checking details:");
            Console.WriteLine($"Source:");
            Console.WriteLine($"{sourceFolder}");
            Console.WriteLine("#############################################");
            Console.WriteLine("Targets:");
            for (int i = 1; i < argList.Count; i++)
            {
                Console.WriteLine($"{argList[i]} ");
            }
            Console.WriteLine("#############################################");

            DateTime startdatetime = DateTime.Now;

            var sourceSearchOption = sourceRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var targetSearchOption = targetRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // Get all file names (without extension) in the source folder (recursive or not)
            var sourceFileNames = Directory.GetFiles(sourceFolder, "*", sourceSearchOption)
                                           .Select(f => Path.GetFileNameWithoutExtension(f))
                                           .ToList();

            List<string> duplicatedFiles = new List<string>();

            // Loop through each target folder
            for (int i = 1; i < argList.Count; i++)
            {
                string targetFolder = argList[i];
                if (!Directory.Exists(targetFolder))
                {
                    Console.WriteLine($"WARNING: Target folder '{targetFolder}' does not exist. Skipping.");
                    continue;
                }

                // Get files in target folder (recursively if requested)
                string[] targetFiles = Directory.GetFiles(targetFolder, "*", targetSearchOption);

                foreach (string targetFile in targetFiles)
                {
                    string targetName = Path.GetFileNameWithoutExtension(targetFile);
                    var tolerance = 0.3;
                    // Check if targetName is contained in any sourceName or vice versa, and length is similar
                    bool foundMatch = sourceFileNames.Any(sourceName =>
                    {
                        double ratio = (double)targetName.Length / sourceName.Length;
                        return (sourceName.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                targetName.IndexOf(sourceName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                               (ratio <= 1 + tolerance && ratio >= 1 - tolerance);
                    });

                    if (foundMatch)
                    {
                        duplicatedFiles.Add(targetFile);
                    }
                }
            }
            DateTime enddatetime = DateTime.Now;
            // Print results
            Console.WriteLine("Results:");
            if (duplicatedFiles.Count == 0)
            {

                Console.WriteLine("No duplicated files found.");
                Console.WriteLine($"It took {(enddatetime - startdatetime).TotalSeconds} seconds to check.");
                return;
            }
            else
            {
                Console.WriteLine($"{duplicatedFiles.Count} Duplicated files found in target folders:");
                Console.WriteLine($"It took {(enddatetime - startdatetime).TotalSeconds} seconds to check.");
                foreach (var file in duplicatedFiles)
                {
                    Console.WriteLine(file);
                }
            }



            // Ask user if want to cleanup
            Console.Write("Do you want me to cleanup (move) the duplicated files? (yes or y for yes, else for no): ");
            string? input = Console.ReadLine();
            if (input == null || !(input.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) || input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Cleanup cancelled by user.");
                return;
            }

            // Ask for target folder to move files to
            Console.Write("Enter the folder path to move the duplicated files to: ");
            string? moveTargetFolder = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(moveTargetFolder))
            {
                Console.WriteLine("No folder path provided. Cleanup cancelled.");
                return;
            }

            moveTargetFolder = moveTargetFolder.Trim();

            try
            {
                if (!Directory.Exists(moveTargetFolder))
                {
                    Directory.CreateDirectory(moveTargetFolder);
                    Console.WriteLine($"Created target folder: {moveTargetFolder}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create target folder. Error: {ex.Message}");
                return;
            }

            int movedCount = 0;
            foreach (var file in duplicatedFiles)
            {
                try
                {
                    string destFileName = Path.Combine(moveTargetFolder, Path.GetFileName(file));
                    string finalDestFileName = destFileName;
                    int copyIndex = 1;
                    // Avoid overwriting files in destination
                    while (File.Exists(finalDestFileName))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(destFileName);
                        string ext = Path.GetExtension(destFileName);
                        finalDestFileName = Path.Combine(moveTargetFolder, $"{fileName} ({copyIndex}){ext}");
                        copyIndex++;
                    }
                    File.Move(file, finalDestFileName);
                    movedCount++;
                    Console.WriteLine($"Moved: {file} -> {finalDestFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to move {file}: {ex.Message}");
                }
            }


            Console.WriteLine($"Cleanup complete. {movedCount} file(s) moved to {moveTargetFolder}.");

        }
    }
}