using System;
using System.Linq;

namespace fbognini.Core.Utilities
{
    public static class MMath
    {

        public static DateTime? Max(params DateTime?[] array)
        {
            if (array == null)
                return null;

            int lenght = array.Count();
            if (lenght == 0)
                return null;

            int k = 0;
            while (array[k] == null && k < lenght) ;
            if (k == lenght) return null;

            DateTime max = array[k].Value;
            for (int i = k + 1; i < lenght; i++)
            {
                if (array[i] != null && array[i].Value > max)
                    max = array[i].Value;
            }

            return max;
        }
    }
}
