namespace Polynomial_merge
{
    internal class Program
    {
        static void Main(string[] args)
        {   
            GlobalConfig.LoadConfig();


            Console.Write("Write tape name: ");
            string? inputLine = Console.ReadLine();

            if (inputLine == null)
            {
                throw new Exception("Input file can't be null");
            }
            Tape Tape1 = new Tape(inputLine);

            int dataOption = 0;
            do
            {
                Console.WriteLine("Choose how to get tape data. Write corresponding number: ");
                Console.WriteLine("[1.] Generate random data.");
                Console.WriteLine("[2.] Write data by hand.");
                Console.WriteLine("[3.] Get data from file.");

                inputLine = Console.ReadLine();

            } while (!int.TryParse(inputLine, out dataOption) || dataOption < 1 || dataOption > 3);

            switch (dataOption)
            {
                case 1:
                    int numberOfRecords = 0;
                    do
                    {

                        Console.WriteLine("How many record you want to generate?");

                    } while (!int.TryParse(Console.ReadLine(), out numberOfRecords));
                    Tape1.PopulateTapeWithData(numberOfRecords);
                    break;
                case 2:
                    Tape1.WriteTapeDataByHand();
                    break;
                case 3:
                    Console.Write("Write path to file: ");
                    inputLine = Console.ReadLine();

                    if (inputLine == null)
                    {
                        throw new Exception("Input file can't be null");
                    }
                    Tape1.ReadDataFromFile(inputLine);
                    break;
            }


            Tape1.PrintTapeData();
            Tape1.PolyphaselSort();
            Tape1.PrintTapeData();

            //File.Delete("exp_data.csv");

            //for (int i = 100; i <= 10000; i+=100)
            //{
            //    File.AppendAllText("exp_data.csv", Convert.ToString(i) + ",");
            //    Tape Tape1= new Tape("test.bin");
            //    Tape1.PopulateTapeWithData(i);
            //    Tape1.PolyphaselSort();
            //    Tape1.DeleteTape();

            //}
        }
    }
}
