// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("svFtIQl3b+aH2gMTQTtDdr0lseFEIGeYmYxUDTjUawDQORVZnbXfGZdYKt4M/v5uu8fKvD7WEr5ebVRxIznVtjvWmxzVcvm+x+00PzlyG2gl9tmbumhUHYVeuNHtpFYwOPRBwpo5Akx6YoWIrKXkXU4h8NDs7X7GevZDoOx6bVEYASwP6PsfQYqAzNuklqpizOJXaZKd5U1fSAvJ0obc2k8Ig98UhMpsHr5ssgoAWRgYD417D6cV8VMV03xM1PLQ1N+V/6KWsCTwQsHi8M3GyepGiEY3zcHBwcXAw+O0lvK5lKwo/h808FNhbBBUpZcV9NbQWHEP3Wqz9OUIs3kr30yYkJpCwc/A8ELBysJCwcHAcmy7tsHRUF8cmiwg+Fl5r8LDwcDB");
        private static int[] order = new int[] { 7,11,9,12,10,5,12,8,10,9,10,13,12,13,14 };
        private static int key = 192;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
