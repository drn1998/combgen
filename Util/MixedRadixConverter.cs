namespace combgen.Util;

public class MixedRadixConverter
{
    private List<int> bases;

    // Constructor that accepts the mixed radix bases as a list
    public MixedRadixConverter(List<int> bases)
    {
        this.bases = bases;
    }

    // Converts a number to the mixed-radix system defined by the bases
    public List<int> ConvertToMixedRadix(int number)
    {
        List<int> mixedRadix = new List<int>(new int[bases.Count]);

        // Process each base starting from the rightmost position (least significant)
        for (int i = bases.Count - 1; i >= 0; i--)
        {
            mixedRadix[i] = number % bases[i];  // Remainder gives the current position value
            number /= bases[i];  // Update the number by dividing by the base
        }

        return mixedRadix;
    }
    
    public int Total()
    {
        int product = 1;
        foreach (var radix in bases)
        {
            product *= radix;
        }
        return product;
    }

    public List<int> Bases()
    {
        List<int> _base = new List<int>();
        List<int> locBases = new List<int>(bases);

        locBases.Reverse();
        
        int product = 1;
        foreach (var radix in locBases)
        {
            _base.Add(product);
            product *= radix;
        }
        
        _base.Reverse();

        return _base;
    }
}