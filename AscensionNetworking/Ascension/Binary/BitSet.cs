using System;

namespace Ascension.Networking
{
    public class BitSet : NetObject
    {
        public const int BITSET_LONGS = 16;
        public static readonly BitSet Full;

        static BitSet()
        {
            Full = new BitSet();

            Full.Bits0 |= ulong.MaxValue;
            Full.Bits1 |= ulong.MaxValue;
            Full.Bits2 |= ulong.MaxValue;
            Full.Bits3 |= ulong.MaxValue;
            Full.Bits4 |= ulong.MaxValue;
            Full.Bits5 |= ulong.MaxValue;
            Full.Bits6 |= ulong.MaxValue;
            Full.Bits7 |= ulong.MaxValue;
            Full.Bits8 |= ulong.MaxValue;
            Full.Bits9 |= ulong.MaxValue;
            Full.Bits10 |= ulong.MaxValue;
            Full.Bits11 |= ulong.MaxValue;
            Full.Bits12 |= ulong.MaxValue;
            Full.Bits13 |= ulong.MaxValue;
            Full.Bits14 |= ulong.MaxValue;
            Full.Bits15 |= ulong.MaxValue;
        }


        ulong Bits0;
        ulong Bits1;
        ulong Bits2;
        ulong Bits3;
        ulong Bits4;
        ulong Bits5;
        ulong Bits6;
        ulong Bits7;
        ulong Bits8;
        ulong Bits9;
        ulong Bits10;
        ulong Bits11;
        ulong Bits12;
        ulong Bits13;
        ulong Bits14;
        ulong Bits15;

        public BitSet(


                ulong bits0

                 ,

                ulong bits1

                 ,

                ulong bits2

                 ,

                ulong bits3

                 ,

                ulong bits4

                 ,

                ulong bits5

                 ,

                ulong bits6

                 ,

                ulong bits7

                 ,

                ulong bits8

                 ,

                ulong bits9

                 ,

                ulong bits10

                 ,

                ulong bits11

                 ,

                ulong bits12

                 ,

                ulong bits13

                 ,

                ulong bits14

                 ,

                ulong bits15


                    )
        {
            this.Bits0 = bits0;
            this.Bits1 = bits1;
            this.Bits2 = bits2;
            this.Bits3 = bits3;
            this.Bits4 = bits4;
            this.Bits5 = bits5;
            this.Bits6 = bits6;
            this.Bits7 = bits7;
            this.Bits8 = bits8;
            this.Bits9 = bits9;
            this.Bits10 = bits10;
            this.Bits11 = bits11;
            this.Bits12 = bits12;
            this.Bits13 = bits13;
            this.Bits14 = bits14;
            this.Bits15 = bits15;
        }



        public BitSet()
        {
        }

        public bool IsZero
        {
            get
            {
                return
                                    (this.Bits0 == 0UL) &&
                                    (this.Bits1 == 0UL) &&
                                    (this.Bits2 == 0UL) &&
                                    (this.Bits3 == 0UL) &&
                                    (this.Bits4 == 0UL) &&
                                    (this.Bits5 == 0UL) &&
                                    (this.Bits6 == 0UL) &&
                                    (this.Bits7 == 0UL) &&
                                    (this.Bits8 == 0UL) &&
                                    (this.Bits9 == 0UL) &&
                                    (this.Bits10 == 0UL) &&
                                    (this.Bits11 == 0UL) &&
                                    (this.Bits12 == 0UL) &&
                                    (this.Bits13 == 0UL) &&
                                    (this.Bits14 == 0UL) &&
                                    (this.Bits15 == 0UL) &&
                                    true;
            }
        }

        public void Set(int bit)
        {
            switch (bit / 64)
            {

                case 0: this.Bits0 |= (1UL << (bit % 64)); break;
                case 1: this.Bits1 |= (1UL << (bit % 64)); break;
                case 2: this.Bits2 |= (1UL << (bit % 64)); break;
                case 3: this.Bits3 |= (1UL << (bit % 64)); break;
                case 4: this.Bits4 |= (1UL << (bit % 64)); break;
                case 5: this.Bits5 |= (1UL << (bit % 64)); break;
                case 6: this.Bits6 |= (1UL << (bit % 64)); break;
                case 7: this.Bits7 |= (1UL << (bit % 64)); break;
                case 8: this.Bits8 |= (1UL << (bit % 64)); break;
                case 9: this.Bits9 |= (1UL << (bit % 64)); break;
                case 10: this.Bits10 |= (1UL << (bit % 64)); break;
                case 11: this.Bits11 |= (1UL << (bit % 64)); break;
                case 12: this.Bits12 |= (1UL << (bit % 64)); break;
                case 13: this.Bits13 |= (1UL << (bit % 64)); break;
                case 14: this.Bits14 |= (1UL << (bit % 64)); break;
                case 15: this.Bits15 |= (1UL << (bit % 64)); break;

                default:
                    throw new IndexOutOfRangeException();
            }

            NetAssert.False(IsZero);
        }

        public void Clear(int bit)
        {
            switch (bit / 64)
            {

                case 0: this.Bits0 &= ~(1UL << (bit % 64)); break;
                case 1: this.Bits1 &= ~(1UL << (bit % 64)); break;
                case 2: this.Bits2 &= ~(1UL << (bit % 64)); break;
                case 3: this.Bits3 &= ~(1UL << (bit % 64)); break;
                case 4: this.Bits4 &= ~(1UL << (bit % 64)); break;
                case 5: this.Bits5 &= ~(1UL << (bit % 64)); break;
                case 6: this.Bits6 &= ~(1UL << (bit % 64)); break;
                case 7: this.Bits7 &= ~(1UL << (bit % 64)); break;
                case 8: this.Bits8 &= ~(1UL << (bit % 64)); break;
                case 9: this.Bits9 &= ~(1UL << (bit % 64)); break;
                case 10: this.Bits10 &= ~(1UL << (bit % 64)); break;
                case 11: this.Bits11 &= ~(1UL << (bit % 64)); break;
                case 12: this.Bits12 &= ~(1UL << (bit % 64)); break;
                case 13: this.Bits13 &= ~(1UL << (bit % 64)); break;
                case 14: this.Bits14 &= ~(1UL << (bit % 64)); break;
                case 15: this.Bits15 &= ~(1UL << (bit % 64)); break;

                default:
                    throw new IndexOutOfRangeException();
            }
        }


        public void Combine(BitSet other)
        {
            this.Bits0 |= other.Bits0;
            this.Bits1 |= other.Bits1;
            this.Bits2 |= other.Bits2;
            this.Bits3 |= other.Bits3;
            this.Bits4 |= other.Bits4;
            this.Bits5 |= other.Bits5;
            this.Bits6 |= other.Bits6;
            this.Bits7 |= other.Bits7;
            this.Bits8 |= other.Bits8;
            this.Bits9 |= other.Bits9;
            this.Bits10 |= other.Bits10;
            this.Bits11 |= other.Bits11;
            this.Bits12 |= other.Bits12;
            this.Bits13 |= other.Bits13;
            this.Bits14 |= other.Bits14;
            this.Bits15 |= other.Bits15;
        }

        public void ClearAll()
        {
            this.Bits0 = 0UL;
            this.Bits1 = 0UL;
            this.Bits2 = 0UL;
            this.Bits3 = 0UL;
            this.Bits4 = 0UL;
            this.Bits5 = 0UL;
            this.Bits6 = 0UL;
            this.Bits7 = 0UL;
            this.Bits8 = 0UL;
            this.Bits9 = 0UL;
            this.Bits10 = 0UL;
            this.Bits11 = 0UL;
            this.Bits12 = 0UL;
            this.Bits13 = 0UL;
            this.Bits14 = 0UL;
            this.Bits15 = 0UL;
        }

        public bool IsSet(int bit)
        {
            ulong b = 1UL << (bit % 64);

            switch (bit / 64)
            {

                case 0: return (this.Bits0 & b) == b;
                case 1: return (this.Bits1 & b) == b;
                case 2: return (this.Bits2 & b) == b;
                case 3: return (this.Bits3 & b) == b;
                case 4: return (this.Bits4 & b) == b;
                case 5: return (this.Bits5 & b) == b;
                case 6: return (this.Bits6 & b) == b;
                case 7: return (this.Bits7 & b) == b;
                case 8: return (this.Bits8 & b) == b;
                case 9: return (this.Bits9 & b) == b;
                case 10: return (this.Bits10 & b) == b;
                case 11: return (this.Bits11 & b) == b;
                case 12: return (this.Bits12 & b) == b;
                case 13: return (this.Bits13 & b) == b;
                case 14: return (this.Bits14 & b) == b;
                case 15: return (this.Bits15 & b) == b;

                default:
                    throw new IndexOutOfRangeException();
            }
        }


        public ulong this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return this.Bits0;
                    case 1: return this.Bits1;
                    case 2: return this.Bits2;
                    case 3: return this.Bits3;
                    case 4: return this.Bits4;
                    case 5: return this.Bits5;
                    case 6: return this.Bits6;
                    case 7: return this.Bits7;
                    case 8: return this.Bits8;
                    case 9: return this.Bits9;
                    case 10: return this.Bits10;
                    case 11: return this.Bits11;
                    case 12: return this.Bits12;
                    case 13: return this.Bits13;
                    case 14: return this.Bits14;
                    case 15: return this.Bits15;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: this.Bits0 = value; break;
                    case 1: this.Bits1 = value; break;
                    case 2: this.Bits2 = value; break;
                    case 3: this.Bits3 = value; break;
                    case 4: this.Bits4 = value; break;
                    case 5: this.Bits5 = value; break;
                    case 6: this.Bits6 = value; break;
                    case 7: this.Bits7 = value; break;
                    case 8: this.Bits8 = value; break;
                    case 9: this.Bits9 = value; break;
                    case 10: this.Bits10 = value; break;
                    case 11: this.Bits11 = value; break;
                    case 12: this.Bits12 = value; break;
                    case 13: this.Bits13 = value; break;
                    case 14: this.Bits14 = value; break;
                    case 15: this.Bits15 = value; break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }

}