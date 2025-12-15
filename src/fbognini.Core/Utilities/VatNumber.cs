using System;

namespace fbognini.Core.Utilities
{
    public class VatNumber
    {
        public static bool FormalCheck(string paramPI)
        {
            paramPI = paramPI.Trim();
            try
            {
                if (paramPI.Length == 11)
                {
                    int tot = 0;
                    int dispari = 0;

                    for (int i = 0; i < 10; i += 2)
                        dispari += int.Parse(paramPI.Substring(i, 1));

                    for (int i = 1; i < 10; i += 2)
                    {
                        tot = (int.Parse(paramPI.Substring(i, 1))) * 2;
                        tot = (tot / 10) + (tot % 10);
                        dispari += tot;
                    }

                    int controllo = int.Parse(paramPI.Substring(10, 1));

                    if (((dispari % 10) == 0 && (controllo == 0))
                       || ((10 - (dispari % 10)) == controllo))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
