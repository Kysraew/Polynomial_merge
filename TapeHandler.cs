using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polynomial_merge
{
    public static class TapeHandler
    {
        public static char maxCharValue = (char)120;
        public static char minCharValue = (char)40;
        public static int minRecordLength = 1;
        public static int maxRecordLength = 30;

        public static void PopulateTapeWithData(this Tape Tape, int recordNumber)
        {
            Tape.SetWriteMode();

            Random rnd = new Random();

            for (int i = 0; i < recordNumber; i++)
            {
                Tape.WriteRecord(GenerateRecord(rnd.Next(minRecordLength, maxRecordLength - 1)));
            }
        }
        public static void ReadDataFromFile(this Tape Tape, string FilePath)
        {
            using StreamReader streamReader = new StreamReader(FilePath);
            string? inputLine = streamReader.ReadLine();
            while (inputLine != null)
            {
                if (inputLine.Length == 0)
                {
                    throw new Exception("Record can't be empty");
                }
                else if (inputLine.Length != inputLine.Distinct().Count())
                {
                    throw new Exception("Record has to contain uniqe characters!");
                }
                else
                {
                    Tape.WriteRecord((inputLine + '\0').ToCharArray());
                }
                inputLine = streamReader.ReadLine();
            }
        }

        public static void WriteTapeDataByHand(this Tape Tape)
        {
            int numberOfRecords = 0;

            Console.WriteLine($"\nWrite Records to Tape: {Tape.FileName}.");
            do
            {
                Console.WriteLine("How many record you wish to write to the tape?\nNumber of records: ");
            } while (int.TryParse(Console.ReadLine(), out numberOfRecords) && numberOfRecords <= 0);

            int recordsWritten = 0;
            while (recordsWritten < numberOfRecords) 
            {
                Console.Write($"{recordsWritten + 1}. ");
                string? inputLine = Console.ReadLine();
                if (inputLine == null)
                {
                    break;
                }
                if (inputLine.Length == 0)
                {
                    Console.WriteLine("Record can not be empty!");
                }
                else if (inputLine.Length != inputLine.Distinct().Count())
                {
                    Console.WriteLine("Record has to contain uniqe characters!");
                }
                else
                {
                    Tape.WriteRecord((inputLine + '\0').ToCharArray());
                    recordsWritten++;
                }
            }
            Console.WriteLine("\n--- Writing to tape has ended succesfully. ---");
        }
        private static char[] GenerateRecord(int lenght)
        {   
            //Generate char sets without duplicates

            if (lenght < 1)
            {
                throw new ArgumentException("Record lenght too short");
            }
            else if ((maxCharValue - minCharValue ) + 1 < lenght)
            {
                throw new ArgumentException("Record too long");
            }


            Random rnd = new Random();

            char[] record = new char[lenght + 1];
            record[lenght] = '\0';
            
            for (int i = 0; i < lenght; i++)
            {
                do
                {
                    record[i] = (char)rnd.Next((int)40, (int)120);
                } while (record.Take(i).Contains(record[i]));
            }
            return record;
        }

        public static int CompareCharSet(char[] firstSet, char[] secondSet)
        {

            HashSet<char> firstSetDiffrence = new HashSet<char>(firstSet);
            HashSet<char> secondSetDiffrence = new HashSet<char>(secondSet);

            firstSetDiffrence.ExceptWith(secondSet);
            secondSetDiffrence.ExceptWith(firstSet);

            char firstSetDiffrenceMaxChar = (firstSetDiffrence.Count > 0) ? firstSetDiffrence.Max():(char)0;
            char secondSetDiffrenceMaxChar = (secondSetDiffrence.Count > 0) ? secondSetDiffrence.Max() : (char)0;

            if (firstSetDiffrenceMaxChar > secondSetDiffrenceMaxChar)
            {
                return 1;
            }
            else if (firstSetDiffrenceMaxChar == secondSetDiffrenceMaxChar)
            {
                return 0;
            }
            else 
            {
                return -1;
            }
        }

        private static void PolyphaselDistribution(Tape Tape1, Tape Tape2, Tape Tape3, out int dummyRuns, out int numberOfPhases, out Tape LongerTape, out Tape ShorterTape, out Tape EmptyTape)
        {
            char[]? record = Tape1.ReadRecord();
            char[]? lastTape2Record = null;
            char[]? lastTape3Record = null;

            int tape2RunsGoal = 1;
            int tape3RunsGoal = 1;

            int tape2Runs = 1;
            int tape3Runs = 1;

            numberOfPhases = 0;

            bool addingToSecondTape = true;
            while (record != null)
            {
                if (addingToSecondTape)
                {
                    if (lastTape2Record != null && CompareCharSet(record, lastTape2Record) == -1)
                    {
                        if (tape2Runs == tape2RunsGoal)
                        {
                            numberOfPhases++;
                            addingToSecondTape = false;
                            tape2RunsGoal += tape3RunsGoal;

                        }
                    }
                }
                else
                {
                    if (lastTape3Record != null && CompareCharSet(record, lastTape3Record) == -1)
                    {
                        if (tape3Runs == tape3RunsGoal)
                        {
                            numberOfPhases++;
                            addingToSecondTape = true;
                            tape3RunsGoal += tape2RunsGoal;

                        }
                    }
                }

                if (addingToSecondTape)
                {
                    if (lastTape2Record != null && CompareCharSet(record, lastTape2Record) == -1)
                    {
                        tape2Runs++;
                    }
                    Tape2.WriteRecord(record);
                    lastTape2Record = record;
                }
                else
                {
                    if (lastTape3Record != null && CompareCharSet(record, lastTape3Record) == -1)
                    {
                        tape3Runs++;
                    }
                    Tape3.WriteRecord(record);
                    lastTape3Record = record;
                }

                record = Tape1.ReadRecord();
            }

            EmptyTape = Tape1;
            if (addingToSecondTape)
            {
                if (tape2Runs + tape3Runs == tape2RunsGoal)
                {
                    dummyRuns = 0;
                    LongerTape = Tape3;
                    ShorterTape = Tape2;
                    numberOfPhases--;

                }
                else
                {
                    dummyRuns = tape2RunsGoal - (tape2Runs);
                    LongerTape = Tape2;
                    ShorterTape = Tape3;
                    
                }
                
            }
            else
            {
                if (tape2Runs + tape3Runs == tape3RunsGoal)
                {
                    dummyRuns = 0;
                    LongerTape = Tape2;
                    ShorterTape = Tape3;
                    numberOfPhases--;
                }
                else
                {
                    dummyRuns = tape3RunsGoal - (tape3Runs);
                    LongerTape = Tape3;
                    ShorterTape = Tape2;
                }
            }
            Tape1.ClearTape();
        }

        private static void PrintTapes(Tape LongerTape, Tape ShorterTape, Tape EmptyTape, char[]? alreadyLoadedShorterRecord)
        {
            Console.WriteLine("In order to see tapes write [y]");
            string? inputLine = Console.ReadLine();
            if (inputLine != null && inputLine == "y")
            {
                EmptyTape.PrintTapeData();
                if(alreadyLoadedShorterRecord != null)
                {
                    ShorterTape.PrintTapeData(alreadyLoadedShorterRecord);
                }
                else
                {
                    ShorterTape.PrintTapeData();
                }
                LongerTape.PrintTapeData();
            }
        }

        public static void PolyphaselSort(this Tape MainTape)
        {
            MainTape.ResetDiscReadsCouter();
            MainTape.ResetDiscWritesCouter();
            MainTape.ResetRecordReadsCouter();
            MainTape.ResetRecordWritesCouter();
            
            Tape Tape2 = new Tape($@"{Guid.NewGuid()}.bin");
            Tape Tape3 = new Tape($@"{Guid.NewGuid()}.bin");

            Tape LongerTape;
            Tape ShorterTape;
            Tape EmptyTape;

            int dummyRuns = 0;
            int numberOfPhases = 0;

            //distribution phase

            PolyphaselDistribution(MainTape, Tape2, Tape3, out dummyRuns, out numberOfPhases, out LongerTape, out ShorterTape, out EmptyTape);

            Console.WriteLine("--- Tape Distributed ---");
            PrintTapes(LongerTape, ShorterTape, EmptyTape, null);


            //Merging phase

            // Taking care of dummy runs

            char[]? shorterTapeRecord = ShorterTape.ReadRecord();
            char[]? longerTapeRecord = null;
            if (shorterTapeRecord == null)
            {
                throw new Exception("Unexpeced end of tape");
            }
            char[]? lastShorterTapeRecord = null;
            char[]? lastLongerTapeRecord = null;

            for (int i = 0; i < dummyRuns; i++)
            {
                lastShorterTapeRecord = null;
                while (true)
                {
                    if (lastShorterTapeRecord == null || CompareCharSet(shorterTapeRecord, lastShorterTapeRecord) >= 0)
                    {
                        EmptyTape.WriteRecord(shorterTapeRecord);

                        lastShorterTapeRecord = shorterTapeRecord;
                        shorterTapeRecord = ShorterTape.ReadRecord();
                        if (shorterTapeRecord == null)
                        {
                            throw new Exception("Unexpeced end of tape");
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < numberOfPhases; i++)
            {
                // In phase 1 record was already loeded during taking care of dummy runs
                // In other phases record from longer tape was already loaded but now longer tape is shorter
                if (i != 0)
                {
                    shorterTapeRecord = longerTapeRecord;
                }
                longerTapeRecord = LongerTape.ReadRecord();


                lastLongerTapeRecord = null;
                lastShorterTapeRecord = null;

                bool longerTapeRunEnded = false;
                bool shorterTapeRunEnded = false;

                while (true)
                {
                    if (shorterTapeRunEnded && longerTapeRunEnded)
                    {
                        if (shorterTapeRecord == null)
                        {
                            break;
                        }
                        shorterTapeRunEnded = false;
                        longerTapeRunEnded = false;
                        
                    }
                    else if(!shorterTapeRunEnded && (longerTapeRunEnded || CompareCharSet(longerTapeRecord, shorterTapeRecord) == 1))
                    {

                        EmptyTape.WriteRecord(shorterTapeRecord);

                        lastShorterTapeRecord = shorterTapeRecord;
                        shorterTapeRecord = ShorterTape.ReadRecord();

                        if (shorterTapeRecord == null || CompareCharSet(shorterTapeRecord, lastShorterTapeRecord) == -1)
                        {
                            shorterTapeRunEnded = true;
                        }
                    }
                    else
                    {
                        EmptyTape.WriteRecord(longerTapeRecord);
                        lastLongerTapeRecord = longerTapeRecord;
                        longerTapeRecord = LongerTape.ReadRecord();

                        if (longerTapeRecord == null || CompareCharSet(longerTapeRecord, lastLongerTapeRecord) == -1)
                        {
                            longerTapeRunEnded = true;
                        }
                    }
                }

                ShorterTape.ClearTape();


                //Change tapes
                Tape temp = EmptyTape;
                EmptyTape = ShorterTape;
                ShorterTape = LongerTape;
                LongerTape = temp;

                Console.WriteLine($"--- Phase {i + 1} ended ---");
                if (longerTapeRecord != null)
                {
                    PrintTapes(LongerTape, ShorterTape, EmptyTape, longerTapeRecord);
                }
                else
                {
                    PrintTapes(LongerTape, ShorterTape, EmptyTape, null);
                }

            }
            
            MainTape.SwapFile(LongerTape);

            //Console.WriteLine("--- Sorting ended ---");
            //Console.WriteLine($"Number of phases: {numberOfPhases}");
            //Console.WriteLine($"Number of dummy runs: {dummyRuns}");
            //Console.WriteLine($"Number of disc writes: {MainTape.WritesCounter + Tape2.WritesCounter + Tape3.WritesCounter}");
            //Console.WriteLine($"Number of disc reads: {MainTape.ReadsCounter + Tape2.ReadsCounter + Tape3.ReadsCounter}");
            //MainTape.PrintTapeData();

            //string line = string.Empty;
            //line += $"{numberOfPhases},";
            //line += $"{dummyRuns},";
            //line += $"{MainTape.DiscWritesCounter + Tape2.DiscWritesCounter + Tape3.DiscWritesCounter},";
            //line += $"{MainTape.DiscReadsCounter + Tape2.DiscReadsCounter + Tape3.DiscReadsCounter},";
            //line += $"{MainTape.RecordWritesCounter + Tape2.RecordWritesCounter + Tape3.RecordWritesCounter},";
            //line += $"{MainTape.RecordReadsCounter + Tape2.RecordReadsCounter + Tape3.RecordReadsCounter}";
            //File.AppendAllText("exp_data.csv", line+ '\n');


            Tape2.DeleteTape();
            Tape3.DeleteTape();

        }

    }
}
