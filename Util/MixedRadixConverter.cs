using System.Numerics;

namespace combgen.Util;

public class MixedRadixConverter
{
    private List<BigInteger> bases;

    // Constructor that accepts the mixed radix bases as a list
    public MixedRadixConverter(List<BigInteger> bases)
    {
        this.bases = bases;
    }

    // Converts a number to the mixed-radix system defined by the bases
    public List<BigInteger> ConvertToMixedRadix(BigInteger number)
    {
        List<BigInteger> mixedRadix = new List<BigInteger>(new BigInteger[bases.Count]);

        // Process each base starting from the rightmost position (least significant)
        for (int i = bases.Count - 1; i >= 0; i--)
        {
            mixedRadix[i] = number % bases[i];  // Remainder gives the current position value
            number /= bases[i];  // Update the number by dividing by the base
        }

        return mixedRadix;
    }
    
    public BigInteger Total()
    {
        BigInteger product = 1;
        foreach (var radix in bases)
        {
            product *= radix;
        }
        return product;
    }

    public List<BigInteger> Bases()
    {
        List<BigInteger> _base = new List<BigInteger>();
        List<BigInteger> locBases = new List<BigInteger>(bases);

        locBases.Reverse();
        
        BigInteger product = 1;
        foreach (var radix in locBases)
        {
            _base.Add(product);
            product *= radix;
        }
        
        _base.Reverse();

        return _base;
    }
}