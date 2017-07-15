using Logic;
using UnityEngine;


namespace Assets.Scripts.Logic.Math{

    public static class Mathfp{

        public static FixedPoint Abs(FixedPoint value){
            if (value < (FixedPoint)0){
                return value * (FixedPoint)(-1);
            }
            return value;
        }

        public static FixedPoint Ceiling(FixedPoint value){
            FixedPoint fraction = value.Fraction();
            if (fraction != 0){
                return value - value.Fraction() + (FixedPoint)(value > 0 ? 1 : 0);
            }
            return value;
        }

        public static FixedPoint Floor(FixedPoint value){
            FixedPoint fraction = value.Fraction();
            if (fraction != 0){
                return value - value.Fraction() - (FixedPoint)(value > 0 ? 0 : 1);
            }
            return value;
        }

        public static FixedPoint Clamp(FixedPoint value, FixedPoint lowerLimit, FixedPoint upperLimit){
            if (value > upperLimit){
                return upperLimit;
            }

            if (value < lowerLimit){
                return lowerLimit;
            }

            return value;
        }

        public static FixedPoint UpperClamp(FixedPoint value, FixedPoint upperLimit){
            if (value > upperLimit){
                return upperLimit;
            }

            return value;
        }

        public static FixedPoint LowerClamp(FixedPoint value, FixedPoint lowerLimit){
            if (value < lowerLimit){
                return lowerLimit;
            }

            return value;
        }

        public static FixedPoint Pow(FixedPoint value, int power){
            if (power > 1){
                return value * Pow(value, power - 1);
            }
            if (power == 1){
                return value;
            }
            if (power == 0){
                return 1;
            }
            if (power == -1){
                return 1 / value;
            }
            if (power < -1){
                return (1 / value) * Pow(value, power + 1);
            }
            throw new UnityException("Fixed point Pow unknown error!");
        }

    }

}