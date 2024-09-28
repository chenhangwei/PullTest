using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace PullTest
{
    public class CustomFontResolver : IFontResolver
    {
        private readonly byte[] _simfangFontData;
        public CustomFontResolver()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("PullTest.Resources.Fonts.simfang.ttf"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Resource 'YourNamespace.simfang.ttf' not found.");
                }
                _simfangFontData = new byte[stream.Length];
                stream.Read(_simfangFontData, 0, (int)stream.Length);
            }
        }
        public byte[]? GetFont(string faceName)
        {
            // 检查请求的字体族名称是否为 "STSong"
            if (faceName.Equals("simfang", StringComparison.OrdinalIgnoreCase))
            {
                // 返回 STSong 字体的二进制数据
                return _simfangFontData;
            }
            return null;
        }
        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            // 检查请求的字体族名称是否为 "STSong"
            if (familyName.Equals("simfang", StringComparison.OrdinalIgnoreCase))
            {
                // 返回一个新的 FontResolverInfo 对象
                return new FontResolverInfo(familyName);
            }
            // 如果不是 STSong 字体，返回 null 或者调用默认的字体解析器
            return null; // 或者 PlatformFontResolver.ResolveTypeface(familyName, bold, italic);
        }
    }
}
