using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Entities;

using UI.Screens.Constants;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using System.Linq;
using System;
using System.Text;
using Genrpg.Shared.MapServer.Entities;
using UnityEngine;

public class CharacterSelectScreen : BaseScreen
{
    
#if UNITY_EDITOR
    public GButton GenWorldButton;
    public GButton NoiseButton;
    public GText NoiseText;
#endif
    public GEntity CharacterGridParent;
    public GButton CreateButton;
    public GButton LogoutButton;
    public GButton QuitButton;

    protected IZoneGenService _zoneGenService;
    protected IClientLoginService _loginService;
    protected INoiseService _noiseService;

    public const string CharacterRowArt = "CharacterSelectRow";

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
#if UNITY_EDITOR

        if (GenWorldButton == null)
        {
            GEntity genWorldObj = GEntityUtils.FindChild(entity, "GenWorldButton");
            if (genWorldObj != null)
            {
                GenWorldButton = GEntityUtils.GetComponent<GButton>(genWorldObj);
            }
        }

        UIHelper.SetButton(GenWorldButton, GetAnalyticsName(), ClickGenerate);
        UIHelper.SetButton(NoiseButton, GetAnalyticsName(), ClickNoise);
#endif
        GEntityUtils.DestroyAllChildren(CharacterGridParent);

        UIHelper.SetButton(LogoutButton, GetAnalyticsName(), ClickLogout);
        UIHelper.SetButton(CreateButton, GetAnalyticsName(), ClickCharacterCreate);
        UIHelper.SetButton(QuitButton, GetAnalyticsName(), ClickQuit);

        SetupCharacterGrid();

        GetSpellIcons(_gs);

        await Task.CompletedTask;
    }

    private void GetSpellIcons(UnityGameState gs)
    {
    }


#if UNITY_EDITOR
    public void ClickGenerate()
    {
        if (_gs.characterStubs.Count < 1)
        {
            FloatingTextScreen.Instance?.ShowError("You need at least one character to generate a map.");
        }
        LoadIntoMapCommand lwd = new LoadIntoMapCommand()
        {
            MapId = InitClient.Instance.CurrMapId,
            CharId = _gs.characterStubs.Select(x => x.Id).FirstOrDefault(),
            GenerateMap = true,
            Env = _gs.Env,
        };
        _zoneGenService.LoadMap(_gs, lwd);
    }

    public void ClickNoise()
    {
        int noiseSize = InitClient.Instance.ZoneNoiseSize;
        float zoneAmp = InitClient.Instance.ZoneNoiseAmplitude;
        float zoneDenom = InitClient.Instance.ZoneNoiseDenominator;
        float pers = InitClient.Instance.ZoneNoisePersistence;
        float lac = InitClient.Instance.ZoneNoiseLacunarity;
        int seed = _gs.rand.Next();
        float[,] heights = _noiseService.Generate(_gs, pers, noiseSize / zoneDenom, zoneAmp, 2, seed, noiseSize, noiseSize, lac);

        int bucketSize = 11;

        Texture2D[] textures = new Texture2D[bucketSize];
        Color[][] pixels = new Color[bucketSize][];

        for (int b = 0; b < bucketSize; b++)
        {
            textures[b] = new Texture2D(noiseSize, noiseSize, TextureFormat.RGB24, true, true);
            pixels[b] = textures[b].GetPixels();
        }

        float[] buckets = new float[bucketSize];

        int totalCells = 0;
        for (int x = 0; x < heights.GetLength(0); x++)
        {
            for (int y = 0; y < heights.GetLength(1); y++)
            {
                float val = (float)Math.Abs(heights[x, y]);
                
                for (int b = 0; b < bucketSize; b++)
                {
                    Color texColor = Color.white;
                    if (val > 1)
                    {
                        val = 1;
                    }

                    if (val <= (1-1.0f*b/(bucketSize-1)))
                    {
                        buckets[b]++;
                        texColor = Color.green;
                    }
                    pixels[b][GetIndex(x, y, noiseSize)] = texColor;
                }

                totalCells++;
            }
        }

        LocalFileRepository repo = new LocalFileRepository(_gs.logger);
        float[] percents = new float[bucketSize];

        for (int b = 0; b < bucketSize; b++)
        {
            percents[b] = 1.0f * buckets[b]/ totalCells;

            textures[b].SetPixels(pixels[b]);

            string filename = "";
            if (b < 10)
            {
                filename = "ZoneNoise0" + b;
            }
            else
            {
                filename = "ZoneNoise" + b;
            }

            filename += ".jpg";

            repo.SaveBytes(filename, textures[b].EncodeToJPG(100));
        }


        StringBuilder sb = new StringBuilder();

        for (int b = 0; b < bucketSize; b++)
        {
            sb.Append(b + ": " + percents[b].ToString("F3") + "\n");
        }

        UIHelper.SetText(NoiseText, sb.ToString());
    }

    private int GetIndex(int x, int y, int noiseSize)
    {
        return x + y * noiseSize;
    }


#endif 

    public void ClickCharacterCreate()
    {
        _screenService.Open(_gs, ScreenId.CharacterCreate);
        _screenService.Close(_gs, ScreenId.CharacterSelect);

    }

    public void OnSelectChar()
    {
        if (!CanClick("selectchar"))
        {
            return;
        }

        CharacterStub currStub = null;

        GEntity selected = UIHelper.GetSelected();

        CharacterSelectRow currRow = null;

        if (selected != null)
        {
            currRow = selected.GetComponent<CharacterSelectRow>();
            if (currRow != null)
            {
                currStub = currRow.GetStub();
            }
        }

    }

    public void ClickLogout()
    {
        _loginService.Logout(_gs);
    }


    public virtual void SetupCharacterGrid()
    {
        if (CharacterGridParent == null)
        {
            return;
        }

        GEntityUtils.DestroyAllChildren(CharacterGridParent);

        foreach (CharacterStub stub in _gs.characterStubs)
        {
            _assetService.LoadAssetInto(_gs, CharacterGridParent, AssetCategory.UI, CharacterRowArt, OnLoadCharacterRow, stub, _token);
        }
    }

    private void OnLoadCharacterRow(UnityGameState gs, string url, object row, object data, CancellationToken token)
    {
        GEntity go = row as GEntity;
        if (go == null)
        {
            return;
        }

        CharacterStub ch = data as CharacterStub;
        if (ch ==null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        CharacterSelectRow charRow = go.GetComponent<CharacterSelectRow>();
        if (charRow == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }
        charRow.Init(ch, this, token);
    }

    public void ClickQuit()
    {
        if (!CanClick("quit"))
        {
            return;
        }
        AppUtils.Quit();
    }

}

