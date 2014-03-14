using System;

namespace FreeBitCoin
{
    public enum BettingSystem : int
    {
        Martingale = 1,
        AntiMartingale,
        Fixed,
        Minimal
    }
    public class CleverBet
    {
        const double min_bet = 0.00000001f;

        double money = 0;
        double diff = 0;
        long wins = 0;
        long loses = 0;
        int multiplier = 2;
        double stake = min_bet;
        double base_stake = min_bet;
        double start = -1;
        BettingSystem bettingSystem = BettingSystem.Martingale;

        bool usePercentBet = false;
        bool slowMode = false;
        bool dryRun = false;

        double loss_margin = 0.7f;
        double win_margin = 0.3f;

        int max_multiplier = 100;
        int min_multiplier = 2;

        int stake_multiplier = 1;

        double least_money = double.MaxValue;
        double most_money = double.MinValue;

        double percent_bet = 0.01f;

        long win_streak = 0;
        long max_win_streak = -1;

        int period = 0;
        String betHiorLo = "hi";

        CodeEvaler codeEvaler;

        private static readonly CleverBet instance = new CleverBet();

        public static CleverBet Instance
        {
            get
            {
                return instance;
            }
        }

        public bool Setup(int _multiplier, double _loss_margin, double _win_margin, double _percent_bet, BettingSystem _bettingSystem, double _stake, bool _slowMode, double _dryRunAmount)
        {
            dryRun = _dryRunAmount > 0;

            multiplier = _multiplier < min_multiplier ? min_multiplier : _multiplier;
            multiplier = _multiplier > max_multiplier ? max_multiplier : _multiplier;

            if (dryRun)
            {
                Dry.SetAmount(_dryRunAmount);
                Dry.Run(_multiplier);
            }
            else
            {
                codeEvaler = new CodeEvaler();
                codeEvaler.Run();
            }

            loss_margin = _loss_margin;
            win_margin = _win_margin;

            percent_bet = _percent_bet;
            bettingSystem = _bettingSystem;
            slowMode = _slowMode;

            if (_stake == 0)
            {
                usePercentBet = true;

                Console.WriteLine("You're using '{8}' strategy with a multiplier of {0}, starting at {1:F8} BTC, a loss margin of {2:P0} or {3:F8} BTC, a win margin of {4:P0} or {5:F8} BTC and a minimum bet of {6:P2} or {7:F8} BTC.",
                    multiplier, start, loss_margin, loss_margin * start, win_margin, (1 + win_margin) * start, percent_bet, percent_bet * money < min_bet ? min_bet : percent_bet * money, Enum.GetName(bettingSystem.GetType(), bettingSystem));
            }
            else
            {
                usePercentBet = false;
                base_stake = _stake;

                Console.WriteLine("You're using '{7}' strategy with a multiplier of {0}, starting at {1:F8} BTC, a loss margin of {2:P0} or {3:F8} BTC, a win margin of {4:P0} or {5:F8} BTC and a minimum bet of {6:F8} BTC.",
                    multiplier, start, loss_margin, loss_margin * start, win_margin, (1 + win_margin) * start, base_stake, Enum.GetName(bettingSystem.GetType(), bettingSystem));
            }

            Console.Write("\nPress \"n\" to stop or any other key to continue: ");
            if (Console.ReadLine().ToLower() == "n")
            {
                Console.WriteLine(" Stopped game by user.\n");
                return false;
            }

            Think();
            return true;
        }

        public bool CanBet()
        {
            bool canBet = money / start > loss_margin && (money - start) / start < win_margin;

            if (!canBet)
                return false;

            PreBet();

            if (slowMode)
            {
                Console.Write("\nPress \"n\" to stop or any other key to continue: ");
                if (Console.ReadLine().ToLower() == "n")
                {
                    Console.WriteLine(" Stopped game by user.\n");
                    return false;
                }
            }

            return true;
        }

        public void Think()
        {
            if (usePercentBet)
                base_stake = money * percent_bet;

            switch (bettingSystem)
            {
                case BettingSystem.Martingale:
                    //double down bet for every loss (favors having lots of moneys)
                    if (diff < 0) //lost last round
                    {
                        IncreaseStake();
                    }
                    else
                    {
                        ReduceStake();
                    }
                    break;

                case BettingSystem.AntiMartingale:
                    //double down bet for every win (favors win streaks)
                    if (diff < 0) //lost last round
                    {
                        ReduceStake();
                    }
                    else
                    {
                        IncreaseStake();
                    }
                    break;

                case BettingSystem.Fixed:
                    //keep bet at a fixed price
                    ReduceStake();
                    break;

                case BettingSystem.Minimal:
                    //keep bet at the least price
                    base_stake = min_bet;
                    ReduceStake();
                    break;
            }
        }

        public void Bet()
        {
            if (dryRun)
            {
                Dry.Run(multiplier);
            }
            else
            {
                codeEvaler.Run();
            }
        }

        private void PreBet()
        {
            period++;
            period = period == multiplier ? 0 : period;

            betHiorLo = new Random().Next(0, 2) == 0 ? "hi" : "lo";

            Console.WriteLine(" You have {0:F8} BTC, about to bet {1:F8} BTC on {7}, could stand to win {2:F8} BTC and {3} last round {4:F8} BTC.{6}{5}",
                money, stake, stake * (multiplier - 1), diff > 0 ? "won " : "lost", Math.Abs(diff), diff > 0 ? "*" : "", period == 0 ? " - " : "   ", betHiorLo);
            Console.WriteLine();
        }

        public void SetMoneys(string data)
        {
            ReadMoneys(data);

            ProcessMoney();
        }

        public void SetDryMoneys(double _money)
        {
            diff = money;
            money = _money;

            ProcessMoney();
        }

        private void ProcessMoney()
        {
            if (start == -1)
            {
                start = money;
                diff = 0;
            }
            else
            {
                diff = money - diff;

                if (diff < 0) //lost last round
                {
                    loses++;
                    win_streak = 0;
                }
                else //won last round
                {
                    wins++;
                    win_streak++;
                    max_win_streak = max_win_streak > win_streak ? max_win_streak : win_streak;
                }
            }

            least_money = least_money < money ? least_money : money;
            most_money = most_money > money ? most_money : money;
        }

        private void ReadMoneys(string data)
        {
            String[] moneys = data.Split(':');

            diff = money;

            if (moneys.Length < 3)
                throw new Exception("Do you have any money? Or an account?");

            if (!double.TryParse(moneys[3], out money))
                throw new Exception("wtf?");
        }

        void ReduceStake()
        {
            //strategy #1
            //stake_multiplier--;
            //stake_multiplier = stake_multiplier < 1 ? 1 : stake_multiplier;

            //strategy #2
            //double down and reset
            stake_multiplier = 1;

            SetStake();
        }

        void IncreaseStake()
        {
            //strategy #1
            //stake_multiplier++;

            //strategy #2
            //double down and reset
            stake_multiplier *= 2;

            SetStake();
        }

        private void SetStake()
        {
            stake = base_stake * stake_multiplier;
            stake = money - stake < start * loss_margin ? money - start * loss_margin : stake;
            stake = stake < min_bet ? min_bet : stake;
        }

        public double GetMoneys()
        {
            return money;
        }

        public double GetMultiplier()
        {
            return multiplier;
        }

        public double GetStake()
        {
            return stake;
        }

        public String GetBetHighOrLow()
        {
            return betHiorLo;
        }

        public string GetStakeFormatted()
        {
            return String.Format("{0:F8}", stake);
        }

        public string GetResults()
        {
            double total = wins + loses;

            Console.WriteLine(" You have {0:F8} BTC, finished this game and                                                   {3} last round {4:F8} BTC.{6}{5}",
                                money, stake, stake * (multiplier - 1), diff > 0 ? "won " : "lost", Math.Abs(diff), diff > 0 ? "*" : "", period == 0 ? " - " : "   ", betHiorLo);
            Console.WriteLine();
            return String.Format("You managed to win {0} times, lose {1} times and {2} {3:F8} BTC.\nYou started with {4:F8} BTC, ended with {13:F8} BTC reached a minimum of {5:F8} BTC and a max of {6:F8} BTC.\nYour multiplier was {7}, so it was expected of the {8} bets you placed, you would win {9:F1} of them and have lost {10:F1} of them.\nIn fact, you found a ratio of {0}/{8}, which means your effective multiplier was {11:F1}.\nYou had a max win streak of {12}.",
                                    wins, loses, money - start > 0 ? "EARN" : "LOSE", Math.Abs(money - start), start, least_money, most_money, multiplier, total, total / multiplier, (total * (multiplier - 1)) / multiplier, total / wins, max_win_streak, money);
        }
    }
}
