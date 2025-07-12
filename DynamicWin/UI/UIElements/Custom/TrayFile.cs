﻿using SkiaSharp;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DynamicWin.Interop;
using DynamicWin.Rendering.Extensions;
using DynamicWin.Rendering.Primitives;
using DynamicWin.UserSettings;

namespace DynamicWin.UI.UIElements.Custom;

internal class TrayFile : UIObject
{
    string file;
    Bitmap thumbnail;

    public string FileName { get => file; }

    DWImage fileIconImage;
    DWText fileTitle;

    bool isSelected = false;
    public bool IsSelected { get => isSelected; }

    public static TrayFile lastSelected;

    Tray tray;

    public TrayFile(UIObject? parent, string file, Vec2 position, Tray tray, UIAlignment alignment = UIAlignment.TopCenter):
        base(parent, position, new Vec2(60, 75), alignment)
    {
        this.file = file;
        SilentSetActive(false);

        this.tray = tray;

        Color = GetColor(Theme.Primary.Override(alpha: 0.45f));
        roundRadius = 7.5f;

        fileTitle = new DWText(this, DWText.Truncate(Path.GetFileNameWithoutExtension(file), 8) + Path.GetExtension(file), new Vec2(0, -10), UIAlignment.BottomCenter)
        {
            Color = Theme.TextSecond,
            TextSize = 11
        };

        AddLocalObject(fileTitle);

        var modifyDate = File.GetLastWriteTimeUtc(file);
        var modifyString = modifyDate.ToString("yy/MM/dd HH:mm");

        var fileSize = MathRendering.GetFileSizeString(file);

        AddLocalObject(new DWText(this, modifyString, new Vec2(0, 7.5f), UIAlignment.BottomCenter)
        {
            Color = Theme.TextThird,
            TextSize = 10f
        });

        AddLocalObject(new DWText(this, fileSize, new Vec2(0, 17.5f), UIAlignment.BottomCenter)
        {
            Color = Theme.TextThird,
            TextSize = 10f
        });

        fileIconImage = new DWImage(this, Resources.FileResources.FileIcon, new Vec2(0, 30), new Vec2(50, 50), UIAlignment.TopCenter);
        fileIconImage.allowIconThemeColor = false;
        fileIconImage.roundRadius = 5f;
        fileIconImage.maskOwnRect = true;

        AddLocalObject(fileIconImage);

        RefreshIcon();
    }

    public void RefreshIcon()
    {
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;

            try
            {
                int THUMB_SIZE = 64;
                thumbnail = ThumbnailProvider.GetThumbnail(
                    file, THUMB_SIZE, THUMB_SIZE, ThumbnailOptions.None);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                System.Diagnostics.Debug.WriteLine("Could not load icon.");

                new Thread(() =>
                {
                    try
                    {
                        Thread.Sleep(1500);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        return;
                    }

                    RefreshIcon();
                }).Start();

            }
            catch (FileNotFoundException fnfE)
            {
                return;
            }
            finally
            {
                SKBitmap bMap = null;
                if (thumbnail != null) bMap = thumbnail.ToSKBitmap();
                else bMap = Resources.FileResources.FileIcon;

                fileIconImage.Image = bMap;

                if (thumbnail != null)
                    thumbnail.Dispose();
            }

            SetActive(true);
        }).Start();
    }

    float cycle = 0f;
    float speed = 5f;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        //Size.X = fileTitle.TextBounds.X;

        cycle += deltaTime * speed;
        Color = Theme.Primary.Override(alpha: MathRendering.Remap((float)Math.Sin(cycle), -1, 1, 0.35f, 0.45f));
    }

    public override void OnMouseDown()
    {
        if (!isSelected) isSelected = true;
        else return;

        wasSelected = false;

        if (isSelected)
        {
            if (KeyboardListener.KeyDown.Contains(Keys.LShiftKey) || KeyboardListener.KeyDown.Contains(Keys.RShiftKey))
            {
                int indexLast = 0;
                if (TrayFile.lastSelected != null)
                    indexLast = tray.fileObjects.IndexOf(TrayFile.lastSelected);

                lastSelected = this;
                var newIndex = tray.fileObjects.IndexOf(this);

                if (newIndex == indexLast) return;

                bool upwards = indexLast < newIndex;

                int start = upwards ? indexLast : newIndex;
                int end = upwards ? newIndex : indexLast;

                for (int i = start; i < end; i++)
                {
                    tray.fileObjects[i].isSelected = true;
                }
            }
            else
            {
                lastSelected = this;
            }
        }
    }

    bool wasSelected = false;

    public override void OnMouseUp()
    {
        if (isSelected && wasSelected)
        {
            isSelected = false;
        }
        else
        {
            wasSelected = true;
            return;
        }
    }

    public override void OnGlobalMouseUp()
    {
        if (!IsHovering)
        {
            if(!(KeyboardListener.KeyDown.Contains(Keys.LControlKey) || KeyboardListener.KeyDown.Contains(Keys.RControlKey)
                                                               || KeyboardListener.KeyDown.Contains(Keys.LShiftKey) || KeyboardListener.KeyDown.Contains(Keys.RShiftKey)))
                isSelected = false;
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        var paint = GetPaint();

        if (isSelected)
        {
            //var rect = SKRect.Create(Position.X - 15, Position.Y, Size.X + 30, Size.Y);
            //var rRect = new SKRoundRect(rect, roundRadius);

            var textR = SKRect.Create(fileTitle.Position.X, fileTitle.Position.Y + 1.5f,
                fileTitle.Size.X, fileTitle.Size.Y);
            textR.Inflate(-2.5f, 0);
            var roundTextRect = new SKRoundRect(textR, 5f);

            var thumbnailR = SKRect.Create(fileIconImage.Position.X, fileIconImage.Position.Y,
                fileIconImage.Size.X, fileIconImage.Size.Y + 5);
            var roundThumbnailRect = new SKRoundRect(thumbnailR, 2.5f);
            roundThumbnailRect.Inflate(5f, 5f);

            var path = new SKPath();
            path.AddRoundRect(roundTextRect);
            path.AddRoundRect(roundThumbnailRect);

            //canvas.DrawRoundRect(roundTextRect, paint);
            canvas.DrawPath(path, paint);
        }
    }
}