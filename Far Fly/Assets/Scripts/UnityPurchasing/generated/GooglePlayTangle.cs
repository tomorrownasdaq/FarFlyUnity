// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("1Ci16op6E7rq5GPDrOx+6xCP6Q4XlJqVpReUn5cXlJSVKSbHnokgjQ9LMQJjZ3NemOOaOWskFUjqBlxOfgExkph7tylQd5e6ciCQM3Hp/6ClF5S3pZiTnL8T3RNimJSUlJCVlvfPS+BrdA9k0O3JvuZGMxMtN1gDJ8QOQU136mFnSANPiuzyPC67lT/21EJWol/uN6kYHKy3SGgDIbmLFSusktNy5DFd44nZTHNqNigI7nESZ5Ugm0HOgOQAuwIjZNlg3KmVVYa9NhaPy6PUvW/8U0knrU/LFgqy/0wf1lUNvQ2Kd3dzHsBTgpqK4I2YCIBt/H7MUs4B+n1aJ7B2dZQikXevLeKjShwlEJoYckiTQnq5LomH+VfOZy9fHoOp2JeWlJWU");
        private static int[] order = new int[] { 5,1,7,9,5,10,6,7,8,13,13,13,13,13,14 };
        private static int key = 149;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
