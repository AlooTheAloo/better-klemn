using System.IO;
using UnityEngine;
using Chroma.Utility;
using System.Linq;

public struct ImageLoaderImages
{
    private const float MIN_COLOR = 0.2f;
    const int PRECISION = 20;
    const int STEP = 5;

    public Texture2D imageBG;
    public Texture2D imageCover;
    public Color mainColor;
    public Color[] colors;
    public ImageLoaderImages(Texture2D imageBG, Texture2D imageCover)
    {
        this.imageBG = imageBG;
        this.imageCover = imageCover;
        colors = ImageAnalyseur.TrouverCouleurPlusPresente(imageBG, STEP, PRECISION);
        Color.RGBToHSV(colors.First(), out float H, out float S, out float V);

        float newV = Mathf.Clamp(V, MIN_COLOR, 1 - MIN_COLOR);
        mainColor = Color.HSVToRGB(H, S, newV);
    }
}

public class ImageLoader
{
    public ImageLoaderImages LoadData(Metadonnees metadonnees)
    {
        return new ImageLoaderImages(
            getImage(metadonnees.fichiers.arrierePlan),
            getImage(metadonnees.fichiers.imageCouverture)
        ); 
    }
    
    public Texture2D getImage(string path)
    {
        var bytes = File.ReadAllBytes(path);

        // Les chiffres ici ne sont pas importants, ils seront override par LoadImage()
        var image = new Texture2D(727, 727); 

        image.LoadImage(bytes);
        return image;
    }

}
