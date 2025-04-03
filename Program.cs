using System;
using System.Diagnostics;
using System.IO;

namespace EDI  //end of day Drawer counter command line interface
{
    internal class Program
    {
        #region Variable Declaration
        public static Stopwatch AppTimer = new Stopwatch();  //time total app execution
        public static double[] Multiplier = new double[] {50,20,10,5,1,  //each category is multiplied by these values
            0.25,0.10,0.05,0.01,10,5,2,0.5, 1 };
        public static string[] MoneyType = new string[] { //money category 
            "       Fifties", "      Twenties", "          Tens",
"         Fives", "          Ones", "      Quarters",
"         Dimes", "       Nickels", "       Pennies",
" Quarter Rolls", "    Dime Rolls", "  Nickel Rolls",
"   Penny Rolls", "Register Total"};
        public static double[] DollarAmounts = new Double[14];  //stores total dollar amount for each category
        public static double[] ValueAmounts = new Double[14];  //tracks number of bills and coins for each type
        public static double GrandTotal, diff;  //grand total, difference between register total and counted total
        public static Double BillsTotal, CoinsTotal, RollsTotal;  //totals for total bills, coins, Rolls
        public static string DrawerState;  //drawer over/short/perfect
        public static string LogPath = Environment.GetEnvironmentVariable("onedriveconsumer") + "\\documents\\brew for you\\End of Day Drawer\\";
        #endregion

        static void Main(string[] args)
        {
            // Get all command-line arguments
            string[] cmdArgs = Environment.GetCommandLineArgs();

            // Check if the parameter '-p' exists
            if (Array.Exists(cmdArgs, arg => arg == "-p"))
            {
                LogPath = Environment.CurrentDirectory + "\\End of Day Drawer\\";  //change directory
            }

            Console.Title = "BFY End of Day Drawer";  //console window title
            Console.ForegroundColor = ConsoleColor.White;  //text color for console
            Print("This is the Brew for You End of Day Drawer Counter.");
            Print("This program helps you speedily count a cash drawer.");
            Print("For each type of money, input the NUMBER counted, NOT the value.");
            Print("EXAMPLE:  for $60 in Fives, the number 12 would be used, not 65.\n\n");
            AppTimer.Start();  //start timer when getting inputs

            GetAmounts();  //get amount for each category
            GetTotal();  //total of money counted
            CompareTotals();  //compare to register receipt total
            MakeChanges();  //make changes if needed
            SaveToFile();  //saves a report to a text file.  one for today, and a file containing all reports
        }

        static double GetNumber(String Prompt, int Low, int High)  //get a number from thee user
        {
            string line; double rtn = 0;  //line read and number returned
            while (true)  //loop until valid input
            {
                Console.Write(Prompt);  //display prompt message before getting input
                line = Console.ReadLine();  //store input

                if (double.TryParse(line, out rtn))  //if string can convert to double
                {
                    // Successfully parsed, exit the loop
                    break;
                }
                else
                {
                    // Invalid input, prompt the user again
                    System.Media.SystemSounds.Asterisk.Play();  //play a sound 
                }
            }

            if (rtn < Low || rtn > High)  //bounds check
            {
                System.Media.SystemSounds.Asterisk.Play();
                GetNumber(Prompt, Low, High);
            }

            return rtn;  //return number, all checks passed
        }

        static void Print(string msg = "")  //write to console.  default is blank line.
        {
            Console.WriteLine(msg);  //write to screen
        }

        static void GetAmounts(int Index)  //get amount for one category only, overload 1 of 2
        {
            ValueAmounts[Index] = GetNumber(MoneyType[Index] + ":  ", 0, 99);  //print money type, takes input
            DollarAmounts[Index] = ValueAmounts[Index] * Multiplier[Index];  //overwrites previous value
        }

        static void GetAmounts()  //get input for all values, overload 2 of 2
        {
            for (int Index = 0; Index < DollarAmounts.Length; Index++)  //loop through all money types
                {
                    ValueAmounts[Index] = GetNumber(MoneyType[Index] + ":  ", 0, 999 );  //get number
                    DollarAmounts[Index] = ValueAmounts[Index] * Multiplier[Index];  //store in array
                }
        }

        static void GetTotal()  //total the entire DollarAmounts array
        {
            GrandTotal = 0;  //grand total loop, resets before addition
            for (int Index = 0; Index < 13 ; Index++)
            {
                GrandTotal += DollarAmounts[Index];    
            }

            BillsTotal = 0;  //total of bills only
            for (int Index = 0; Index < 5; Index++)
            {
                BillsTotal += DollarAmounts[Index];
            }

            CoinsTotal = 0;  //coins total only
            for (int Index = 5; Index < 9; Index++)
            {
                CoinsTotal += DollarAmounts[Index];
            }

            RollsTotal = 0;  //coin rolls total only
            for (int Index = 9; Index < 13; Index++)
            {
                RollsTotal += DollarAmounts[Index];
            }
        }

        static void CompareTotals()  //compare register total to total
        {
            double RegisterTotal = DollarAmounts[13];  //register total
            if (GrandTotal > RegisterTotal)  //drawer has extram money
            {
                diff = GrandTotal - RegisterTotal;  //difference between values
                DrawerState = "The drawer is over by $" + diff.ToString("n2");
            }
            if (RegisterTotal > GrandTotal)  //drawer short
            {
                diff = RegisterTotal - GrandTotal;
                DrawerState ="The drawer is Short by $" + diff.ToString("n2");
            }
            if (RegisterTotal == GrandTotal)  //drawer matches register total
            {
                DrawerState = "The Drawer is Perfect!";
            }

            //print results of comparison
            Print("\n\nThe Grand Total is $" + GrandTotal + ".  " + DrawerState);
        }

        static void MakeChanges()  //make changes to a category if needed
        {
            Print("\nDo you need to make any changes?  (Y or N)");  //prompt
            String input = Console.ReadLine();  //read a line from keyboard
            if (input.ToUpper().Equals("Y"))  //if first letter is Y
            {
                Print();  //choose a category to cange by number, 14 to exit this menu
                Print("00 - Fifties            01 - Twenties            02 - Tens");
                Print("03 - Fives              04 - Ones                05 - Quarters");
                Print("06 - Dimes              07 - Nickels             08 - Pennies");
                Print("09 - Quarter Rolls      10 - Dime Rolls          11 - Nickel Rolls");
                Print("12 - Penny Rolls        13 - Register Total      14 - Exit");
                double Index = GetNumber("Choose an item to change from 0 to 14:  ", 0, 14);
                if (Index >= 0 && Index < 14)  //value between 0 and 13
                {
                    GetAmounts((int)Index);  //get new value for selected category
                    GetTotal(); //update totals
                    CompareTotals();  //compare totals after change
                    MakeChanges();  //ask again for changes, just in case you did lots of oopsies
                }
                else if (Index == 14)  //exit menu.
                {
                    CompareTotals();  //print grand total before exiting
                    return;
                }
                else
                {
                    MakeChanges();  //number bot between 0-14
                }    
            }
        }

        static void SaveToFile ()  //save everything to a text file
        {
            AppTimer.Stop();  //stop timer
            TimeSpan ts = AppTimer.Elapsed;  //timespan for elapsed time
            String TimeMessage = String.Format("{0} Hours, {1} Minutes, {2} Seconds",
            ts.Hours, ts.Minutes, ts.Seconds);  //store total time.  If it takes you an hour, immediately report to your doctor
            Print("      Count Time:  " + TimeMessage); //print timespan

            //path to save reports Onedrive\Documents\brew for you\end of day imcome folder
            Directory.CreateDirectory(LogPath);  //create directory if not present

            //Today log file full path, report for today only
            string TodayLogPath = LogPath + "Today.txt";
            //Full log file path, contains all reports, oldest to newest
            string FullLogPath = LogPath + "Full.txt";
            
            //open todaylog for writing, overwrites existing file
            System.IO.StreamWriter TodayLog = new System.IO.StreamWriter(TodayLogPath);
            DateTime end = DateTime.Now;  

            TodayLog.WriteLine("End of Day Drawer Report");
            TodayLog.WriteLine(end);  //print date
            TodayLog.WriteLine();

            for (int Index = 0; Index < 12; Index++)  //loop through all money categories
            {
                if (DollarAmounts[Index] > 0)  //if value not zero,
                {
                    //print category name, value, and dollar amount
                    TodayLog.WriteLine(ValueAmounts[Index] +  " " + MoneyType[Index] + " = $" + DollarAmounts[Index].ToString("n2"));
                }
            }
            TodayLog.WriteLine();
            TodayLog.WriteLine("     Bills Total: $" + BillsTotal);  //write totals
            TodayLog.WriteLine("     Coins Total: $" + CoinsTotal);
            TodayLog.WriteLine("     Rolls Total: $" + RollsTotal);
            TodayLog.WriteLine("     Grand Total: $" + GrandTotal);
            TodayLog.WriteLine("  Register Total: $" + DollarAmounts[DollarAmounts.Length - 1]);
            TodayLog.WriteLine("      Count Time:  " + TimeMessage);
            TodayLog.WriteLine(DrawerState);
            TodayLog.WriteLine("==============================");
            TodayLog.Close();  //close file, finished writing data

            //add contents of Todaylog and append it to FullLog
            string TodayLogContents = File.ReadAllText(TodayLogPath);
            File.AppendAllText(FullLogPath, TodayLogContents);
        }

    } 
}