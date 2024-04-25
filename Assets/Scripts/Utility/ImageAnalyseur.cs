using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.Utility
{
    public class ImageAnalyseur : MonoBehaviour
    {
        const int COULEURS_A_RETOURNER = 50;

        [Button]
        public static Color[] TrouverCouleurPlusPresente(Texture2D texture, int step, int precision)
        {
            return HeightRange(0, texture.height, step)
                .SelectMany(y => WidthRange(0, texture.width, step).Select(x => texture.GetPixel(x, y)))
                .Select(c =>
                {
                    c.r = Mathf.Round(c.r * precision) / precision;
                    c.g = Mathf.Round(c.g * precision) / precision;
                    c.b = Mathf.Round(c.b * precision) / precision;
                    c.a = 1;
                    return c;
                })
                .GroupBy(c => c)
                .OrderByDescending(group => group.Count())
                .Take(COULEURS_A_RETOURNER)
                .Select(group => group.Key)
                .ToArray();
        }

        private static IEnumerable<int> HeightRange(int start, int end, int step)
        {
            return Enumerable.Range(start, (end - start) / step + 1).Select(i => i * step);
        }

        private static IEnumerable<int> WidthRange(int start, int end, int step)
        {
            return Enumerable.Range(start, (end - start) / step + 1).Select(i => i * step);
        }
    }
}
