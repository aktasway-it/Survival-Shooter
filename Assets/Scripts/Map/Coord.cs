using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
    public struct Coord 
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }

        public static Coord operator +(Coord c1, Coord c2)
        {
            return new Coord(c1.x + c2.x, c1.y + c2.y);
        }

        public static Coord operator -(Coord c1, Coord c2)
        {
            return new Coord(c1.x - c2.x, c1.y - c2.y);
        }

        public static Coord operator *(Coord c1, int i)
        {
            return new Coord(c1.x * i, c1.y * i);
        }

        public override bool Equals(object obj)
        {
            if (obj is Coord)
            {
                Coord c = (Coord)obj;
                return this == c;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }
    }

