// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("4cPFTWQayH+m4fAdpmw+ylmNhY9X1NrV5VfU39dX1NTVZ3muo9TERTDjzI6vfUEIkEutxPixQyUt4VTXGrIA5EYAxmlZwefFwcqA6reDpTH2oYPnrIG5PesKIeVGdHkFQbCCAOVX1Pfl2NPc/1OdUyLY1NTU0NXWUTVyjYyZQRgtwX4VxSwATIigygw2LMCjLsOOCcBn7KvS+CEqLGcOfafkeDQcYnrzks8WBlQuVmOoMKT0b+NWtflveEQNFDka/e4KVJ+V2c6PLBdZb3eQnbmw8UhbNOXF+fhr01odlsoBkd95C6t5px8VTA0NGphusYO/d9n3QnyHiPBYSl0e3MeTyc+CTT/LGevre67S36krwwerS3hBZEoJjzk17UxsutfW1NXU");
        private static int[] order = new int[] { 6,1,10,10,13,6,11,12,12,10,11,13,12,13,14 };
        private static int key = 213;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
