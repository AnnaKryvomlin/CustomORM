using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight.DLL.Entities
{
    public class GridObject<T>:IComparable
    {
        public T Object;
        public int xStart { get; private set; }
        public int xEnd { get; private set; }
        public int yStart { get; private set; }
        public int yEnd { get; private set; }
        public double distanceToGridCenter { get; private set; }

        public int CompareTo(object o)
        {
            GridObject<Ship> ship = o as GridObject<Ship>;
            if (ship != null)
                return this.distanceToGridCenter.CompareTo(ship.distanceToGridCenter);
            else
                throw new Exception("Невозможно сравнить два объекта");
        }

        public GridObject(T obj, int xStart, int xEnd, int yStart, int yEnd, double xСenter, double yCenter)
        {
            Object = obj;
            this.xStart = Math.Abs(xStart);
            this.xEnd = Math.Abs(xEnd);
            this.yStart = Math.Abs(yStart);
            this.yEnd = Math.Abs(yEnd);
            distanceToGridCenter = FindDistanceToCenter(xСenter, yCenter);
        }
   
        public static void CheckLength(int length, int xStart, int xEnd, int yStart, int yEnd)
        {
            if (xStart > xEnd || yStart > yEnd)
            {
                ++xStart;
                ++yStart;
            }
            else
            {
                ++xEnd;
                ++yEnd;
            }
            int calcLength;
            if (Math.Abs(xEnd - xStart) > 1 && Math.Abs(yEnd - yStart) > 1)
            {
                throw new Exception("Объект должен составлять отрезок");
            }
            else if (Math.Abs(xEnd - xStart) > 1)
            {
                calcLength = Math.Abs(xEnd - xStart);
            }
            else if (Math.Abs(yEnd - yStart) > 1)
            {
                calcLength = Math.Abs(yEnd - yStart);
            }
            else
            {
                calcLength = 1;
            }
            if(calcLength != length)
            {
                throw new Exception("Размер объекта не соответсвует координатам");
            }
        }

        public double FindDistanceToCenter(double xCenter, double yCenter)
        {
            if (this.xStart == xCenter)
                return this.yStart;
            if (this.yStart == yCenter)
                return this.xStart;
            double xlength = Math.Abs(this.xStart - xCenter);
            double ylength = Math.Abs(this.yStart - yCenter);
            return Math.Sqrt(Math.Pow(xlength, 2) + Math.Pow(ylength, 2));
        }
    }
}
