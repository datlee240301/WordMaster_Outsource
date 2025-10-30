// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("O0GGboEuo/DQlx2wa2KHUQYh1PMNnEHvsU832jVcpJ6pVqtFyb7tDNWctLc6BHXeSMosYXZiuhZgAGJTxEdJRnbER0xExEdHRpuds8KRoJn2rUy+0mZZ+tMqQCFY9W9/GhOa9RswqDjQJebG/6LG5m1NOonSCGP6dsRHZHZLQE9swA7AsUtHR0dDRkXreV8pztoRLZ6MKSgHT+KWwGyg7hhMjXSHcZz+mFTI6zFweTUKSkAdEDaSKaHtSV6TQLu7A0izv72r8LLLhWEtr9PsXmLl2y21E5B76ZkKMluckXSngFWZMrhMigpVhuVpjJlOMRwdxAXVS5eAdba9qeFEmkV7JlX5Db/FSAgH/wCChJ9Vgkhv0jmqXnnru8cnFoCLJURFR0ZH");
        private static int[] order = new int[] { 10,8,7,8,8,6,10,8,9,9,12,12,12,13,14 };
        private static int key = 70;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
