using System;

namespace FreeBitCoin
{
    public static class Dry
    {
        static double moneys = 0.00100000f;

        public static void SetAmount(double _moneys)
        {
            moneys = _moneys;
        }

        public static void Run(int multiplier)
        {
            if (new Random().Next(0, multiplier) == 0)
            {
                //win
                moneys = moneys + CleverBet.Instance.GetStake() * (multiplier - 1);
            }
            else
            {
                //loss
                moneys = moneys - CleverBet.Instance.GetStake();
            }

            CleverBet.Instance.SetDryMoneys(moneys);
        }
    }
}
