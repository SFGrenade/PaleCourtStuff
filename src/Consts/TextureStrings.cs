using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PaleCourtStuff.Consts;

public class TextureStrings
{
    #region Misc
    public const string FLogoKey = "FortniteLogo";
    private const string FLogoFile = "PaleCourtStuff.Resources.FortniteLogo.png";
    public const string ULogoKey = "MadeWithUnityLogo";
    private const string ULogoFile = "PaleCourtStuff.Resources.MadeWithUnity.png";
    public const string SLogoKey = "SilksongLogo";
    private const string SLogoFile = "PaleCourtStuff.Resources.SilksongLogo.png";
    public const string LogoKey = "PaleCourtLogo";
    private const string LogoFile = "PaleCourtStuff.Resources.PaleCourtLogo.png";
    public const string LogoNewKey = "PaleCourtLogoNew";
    private const string LogoNewFile = "PaleCourtStuff.Resources.PaleCourtLogoNew.png";
    public const string LogoNewNewKey = "PaleCourtLogoNewNew";
    private const string LogoNewNewFile = "PaleCourtStuff.Resources.PaleCourtLogoNewNew.png";
    public const string HkLogoBlackKey = "HKLogoBlack";
    private const string HkLogoBlackFile = "PaleCourtStuff.Resources.HKLogoBlack.png";
    public const string DlcListKey = "DlcList";
    private const string DlcListFile = "PaleCourtStuff.Resources.DlcList.png";
    public const string PetalKey = "Petal";
    private const string PetalFile = "PaleCourtStuff.Resources.Petal.png";
    public const string FaderKey = "Black Fader tot";
    private const string FaderFile = "PaleCourtStuff.Resources.Black Fader tot.png";
    #endregion Misc

    private readonly Dictionary<string, Sprite> _dict;

    public TextureStrings()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        _dict = new Dictionary<string, Sprite>();
        string[] tmpTextureFiles = {
            FLogoFile,
            ULogoFile,
            SLogoFile,
            LogoFile,
            LogoNewFile,
            LogoNewNewFile,
            HkLogoBlackFile,
            DlcListFile,
            PetalFile,
            FaderFile
        };
        string[] tmpTextureKeys = {
            FLogoKey,
            ULogoKey,
            SLogoKey,
            LogoKey,
            LogoNewKey,
            LogoNewNewKey,
            HkLogoBlackKey,
            DlcListKey,
            PetalKey,
            FaderKey
        };
        for (int i = 0; i < tmpTextureFiles.Length; i++)
        {
            using Stream s = asm.GetManifestResourceStream(tmpTextureFiles[i]);
            if (s == null) continue;

            byte[] buffer = new byte[s.Length];
            s.Read(buffer, 0, buffer.Length);
            s.Dispose();

            //Create texture from bytes
            Texture2D tex = new Texture2D(2, 2);

            tex.LoadImage(buffer, true);

            // Create sprite from texture
            // Split is to cut off the PaleCourtStuff.Resources. and the .png
            if (tmpTextureFiles[i] == DlcListFile || tmpTextureFiles[i] == HkLogoBlackFile)
            {
                _dict.Add(tmpTextureKeys[i], Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 200f / 3f));
            }
            else
            {
                _dict.Add(tmpTextureKeys[i], Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
            }
        }
    }

    public Sprite Get(string key)
    {
        return _dict[key];
    }
}