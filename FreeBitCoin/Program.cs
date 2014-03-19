using System;

namespace FreeBitCoin
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the automatic freebitco.in multiplier.\nLet's begin ;-)\n");

            int multiplier = 2;
            double loss_margin = 0.7f;
            double win_margin = 0.3f;
            double percent_bet = 0.01f;
            double stake = 0;
            bool slowMode = false;
            double dryRunAmount = 0;
            string cookie = "";
            BettingSystem bettingSystem = BettingSystem.Martingale;

            try
            {
                if (args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "-x":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    multiplier = int.Parse(args[i + 1]);
                                break;
                            case "-m":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    bettingSystem = (BettingSystem)int.Parse(args[i + 1]);
                                break;
                            case "-l":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    loss_margin = double.Parse(args[i + 1]) / 100;
                                break;
                            case "-w":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    win_margin = double.Parse(args[i + 1]) / 100;
                                break;
                            case "-p":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    percent_bet = double.Parse(args[i + 1]) / 100;
                                break;
                            case "-b":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                {
                                    if (!double.TryParse(args[i + 1], out stake) && args[i + 1].ToLower() == "min")
                                        stake = 0.00000001f;
                                }
                                break;
                            case "-s":
                                slowMode = true;
                                break;
                            case "-d":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    dryRunAmount = double.Parse(args[i + 1]);
                                break;
                            case "-c":
                                if (i < args.Length - 1 && args[i + 1] != null)
                                    cookie = args[i + 1];
                                break;
                            case "-h":
                                Console.WriteLine("Use the following options:\n -c \"cookie\" [none]\t\tSet the cookie that needs to be used. Use double quotes to wrap the entire string and seperate values with a semicolon.\n -l Number   [70]\t\tPercent of starting amount least to reach (loss).\n -w Number   [30]\t\tPercent of profit most to reach (win).\n -b Number   [0]  or min\tSet the game's minimum bet. Use 'min' for 0.00000001 BTC. (Overrides any percent bet, -p).\n -p Number   [1]\t\tSet the game's minimum bet based on percentage of total money.\n -m Number   [1]\t\tSet the game's betting strategy. Use 1 For Martingale, 2 for Anti-Martingale, 3 for Fixed bet or 4 for Minimal bet.\n -d Number   [0]\t\tSimulate the game (dry run).\n -s      \t\t\tGo slowly, confirming each bet.\n -h      \t\t\tShow this help.");
                                return;
                        }
                    }
                }

                if (!CleverBet.Instance.Setup(cookie, multiplier, loss_margin, win_margin, percent_bet, bettingSystem, stake, slowMode, dryRunAmount))
                    return;

                Console.WriteLine();

                Console.CancelKeyPress += Console_CancelKeyPress;

                while (CleverBet.Instance.CanBet())
                {                    
                    CleverBet.Instance.Bet();
                    CleverBet.Instance.Think();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured, {0}", ex.Message);
            }

            Console.WriteLine(CleverBet.Instance.GetResults());
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine(" Cancelled game by user.\n");
            Console.WriteLine(CleverBet.Instance.GetResults());
        }
    }
}