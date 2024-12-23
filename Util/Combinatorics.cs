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
    public static int nCr(int n, int k)
    {
        if (k > n) return 0;
        if (k == 0 || k == n) return 1;

        k = Math.Min(k, n - k); // Take advantage of symmetry
        int result = 1;

        for (int i = 1; i <= k; i++)
        {
            result *= n--;
            result /= i;
        }

        return result;
    }
}