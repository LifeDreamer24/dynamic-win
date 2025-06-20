﻿using DynamicWin.Main;
using DynamicWin.Resources;
using DynamicWin.UI.UIElements;
using DynamicWin.UI.UIElements.Custom;
using DynamicWin.Utils;
using System.Runtime.InteropServices;
using DynamicWin.Interop;

namespace DynamicWin.UI.Menu.Menus;

internal class VolumeAdjustMenu : BaseMenu
{
    private static VolumeAdjustMenu instance;

    float shakeStrength = 0f;
    float islandScale = 1.25f;
    
    DWImage volumeImage;
    UIObject mutedBg;
    DWText muteText;
    DWProgressBar volume;
    
    public VolumeAdjustMenu()
    {
        instance = this;
        timerUntilClose = 0f;
        shakeStrength = 0f;
    }
    

    public override List<UIObject> InitializeMenu(IslandObject island)
    {
        var objects = base.InitializeMenu(island);

        mutedBg = new UIObject(island, new Vec2(10, 0), new Vec2(40, 20), UIAlignment.MiddleLeft);
        mutedBg.Color = Theme.Error;
        mutedBg.roundRadius = 15;
        mutedBg.Anchor.X = 0;
        objects.Add(mutedBg);

        volumeImage = new DWImage(island, Resources.FileResources.VolumeOn, new Vec2(20, 0), new Vec2(20, 20), UIAlignment.MiddleLeft);
        volumeImage.Anchor.X = 0;
        objects.Add(volumeImage);

        muteText = new DWText(island, "Silent", new Vec2(-15, 0), UIAlignment.MiddleRight);
        muteText.Anchor.X = 1;
        muteText.TextSize = 15;
        muteText.Font = Resources.FileResources.InterBold;
        muteText.Color = Theme.Error;
        objects.Add(muteText);

        volume = new DWProgressBar(island, new Vec2(-20, 0), new Vec2(150, 5f), UIAlignment.MiddleRight);
        volume.Anchor.X = 1;
        objects.Add(volume);

        return objects;
    }

    public static float timerUntilClose = 0f;

    float timer = 0;
    float shakeSpeed = 35;

    float Func(float x)
    {
        //double result = -Math.Pow(x, 3f) + (-Math.Pow(x - 1, 6f * 4)) + 1;
        //double result = -Math.Pow((x / 0.5) - 1, 2) + 1;
        double result = 1f - (x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2);

        return (float)result;
    }

    float seconds = 0f;
    bool mute = false;

    public override void Update()
    {
        base.Update();

        if (timerUntilClose > 2.75f) MenuManager.CloseOverlay();

        islandScale = Mathf.Lerp(islandScale, 1f, 5f * RendererMain.Instance.DeltaTime);

        var volume = GetVolumePercent();
        var isMuted = IsMuted();

        if (volume <= 0f || isMuted)
        {
            if (!mute)
            {
                mute = true;
                islandScale = 1.05f;
            }

            volumeImage.Image = Resources.FileResources.VolumeOff;
            RendererMain.Instance.MainIsland.LocalPosition.X = Mathf.Lerp(RendererMain.Instance.MainIsland.LocalPosition.X,
                0, 10f * RendererMain.Instance.DeltaTime);

            muteText.SetActive(true);
            mutedBg.SetActive(true);
            this.volume.SetActive(false);
        }
        else if (volume >= 0f && !isMuted)
        {
            if (mute)
            {
                seconds = 0f;
                timer = 0f;
                islandScale = 1.25f;

                mute = false;
            }

            this.volume.value = 0f;

            RendererMain.Instance.MainIsland.LocalPosition.X = Mathf.Lerp(RendererMain.Instance.MainIsland.LocalPosition.X,
                (float)Math.Sin(timer) * shakeStrength, 10f * RendererMain.Instance.DeltaTime);

            volumeImage.Image = Resources.FileResources.VolumeOn;

            muteText.SetActive(false);
            mutedBg.SetActive(false);
            this.volume.SetActive(true);
        }

        timer += RendererMain.Instance.DeltaTime * shakeSpeed;
        seconds += RendererMain.Instance.DeltaTime;

        shakeStrength = Mathf.Clamp(Func(Mathf.Clamp(seconds * 1.5f, 0, 1)), 0, 1f) * 45;

        timerUntilClose += RendererMain.Instance.DeltaTime;

        this.volume.value = (float)volume / 100f;

        var volXOffset = KeyboardListener.keyDown.Contains(System.Windows.Forms.Keys.VolumeUp) ? 2f :
            KeyboardListener.keyDown.Contains(System.Windows.Forms.Keys.VolumeDown) ? -2f : 0;

        this.volume.LocalPosition.X = Mathf.Lerp(this.volume.LocalPosition.X, volXOffset, 
            (Math.Abs(volXOffset) > Math.Abs(this.volume.LocalPosition.X) ? 4.5f : 2.5f) * RendererMain.Instance.DeltaTime);
    }

    public override Vec2 IslandSize()
    {
        return new Vec2(250, 35) * islandScale;
    }

    public override Vec2 IslandSizeBig()
    {
        return base.IslandSizeBig() * 1.05f;
    }

    public override Color IslandBorderColor()
    {
        return IsMuted() ? new Color(0.85f, 0.1f, 0.15f) : new Color(0.5f, 0.5f, 0.5f);
    }
    
    private static double GetVolumePercent()
    {
        var masterVolume = AudioDevices.GetOutputDeviceOrDefault()?.AudioEndpointVolume.MasterVolumeLevelScalar
                           ?? 1f;
        
        return masterVolume * 100;
    }

    private static bool IsMuted()
    {
        return AudioDevices.GetOutputDeviceOrDefault()?.AudioEndpointVolume.Mute
               ?? true;
    }
}