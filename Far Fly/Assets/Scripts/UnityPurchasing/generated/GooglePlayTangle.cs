// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("vsHxUli7d+mQt1d6suBQ87EpP2DXVFpVZddUX1fXVFRV6eYHXkngTchArTy+DJIOwTq9mudwtrVU4lG3ZddUd2VYU1x/0x3TolhUVFRQVVanVeBbgQ5AJMB7wuOkGaAcaVWVRucEzoGNtyqhp4jDj0osMvzue1X/jN8Wlc19zUq3t7PeAJNCWkogTVjrbFITsiTxnSNJGYyzqvboyC6x0s+L8cKjp7OeWCNa+avk1YgqxpyOffbWTwtjFH2vPJOJ522PC9bKcj8U6HUqSrrTeiokowNsLL4r0E8pzjYUgpZiny73adjcbHeIqMPheUvVNw+LIKu0z6QQLQl+Jobz0+33mMNv7SJjitzl0FrYsohTgrp57klHOZcOp++f3kNpGFdWVFVU");
        private static int[] order = new int[] { 9,1,11,9,13,6,13,8,8,9,13,11,13,13,14 };
        private static int key = 85;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
