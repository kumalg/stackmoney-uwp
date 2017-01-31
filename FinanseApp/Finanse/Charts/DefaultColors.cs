using System;
using System.Collections.Generic;
using Windows.UI;

namespace Finanse.Charts
{
    internal class DefaultColors : List<Color>
    {
        private static readonly Color[] _darkArray = new Color[]
            {
                Color(0xD13438),
                Color(0x486860),
                Color(0x4A5459),
                Color(0x5D5A58),

                Color(0x2D7D9A),
                Color(0x0063B1),
                Color(0x6B69D6),
                Color(0x744DA9),

                Color(0xE81123),
                Color(0xDA3801),
                Color(0xCA5010),
                Color(0xFF8C00),

                Color(0x881798),
                Color(0x9A0089),
                Color(0xBF0077),
                Color(0xC30052),

                Color(0x107C10),
                Color(0x10893E),
                Color(0x018574),
                Color(0x038387),

                Color(0x525E54),
                Color(0x7E735F),
                Color(0x515C68),
                Color(0x4C4A48),
            };

        private static readonly Color[] _lightArray = new Color[]
            {
                Color(0xFF4343),
                Color(0x567C73),
                Color(0x69797E),
                Color(0x7A7574),

                Color(0x0099BC),
                Color(0x0078D7),
                Color(0x8E8CD8),
                Color(0x8764B8),

                Color(0xE74856),
                Color(0xEF6950),
                Color(0xF7630C),
                Color(0xFFB900),

                Color(0xB146C2),
                Color(0xC239B3),
                Color(0xE300BC),
                Color(0xEA005E),

                Color(0x498205),
                Color(0x00CC6A),
                Color(0x00B294),
                Color(0x00B7C3),

                Color(0x647C64),
                Color(0x847545),
                Color(0x68768A),
                Color(0x767676),
            };

        // TODO: WHITETHEME
        private static Color[] Array => _lightArray;

        public DefaultColors() : base(Array) { }

        private static Color Color(int i)
        {
            var bytes = BitConverter.GetBytes(i);
            return Windows.UI.Color.FromArgb(0xff, bytes[2], bytes[1], bytes[0]);
        }

        private static Random _rnd;
        private static Color _last;

        public static Color GetRandom()
        {
            if (_rnd == null) _rnd = new Random();
            var color = _last;
            while (color == _last)
                color = Array[_rnd.Next(0, Array.Length - 1)];
            _last = color;
            return _last;
        }
    }
}
