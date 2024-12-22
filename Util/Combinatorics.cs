namespace combgen.Util;

public class Combinatorics
{
    public static int nPr(int n, int r)
    {
        if (r > n || n < 0 || r < 0) throw new ArgumentException("Unable to calculate nPr: Invalid values for n and r.");
        
        if (r == 0) return 1;

        int result = 1;
        for (int i = 0; i < r; i++) result *= (n - i);

        return result;
    }
}