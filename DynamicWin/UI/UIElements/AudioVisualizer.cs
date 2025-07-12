﻿using DynamicWin.Interop;
using DynamicWin.Rendering.Primitives;
using DynamicWin.UserSettings;
using NAudio.Wave;
using SkiaSharp;

namespace DynamicWin.UI.UIElements;

public class AudioVisualizer : UIObject
{
    private int fftLength;
    private float[] fftValues;
    private WasapiLoopbackCapture capture;
    private readonly object fftLock = new object();

    private float[] barHeight;
    public float barUpSmoothing = 100f;
    public float barDownSmoothing = 10f;

    public float divisor = 1f;

    bool avAmpsMode = false;

    public AudioVisualizer(
        UIObject? parent, 
        Vec2 position, 
        Vec2 size, 
        UIAlignment alignment = UIAlignment.TopCenter, 
        int length = 16, 
        int averageAmpsSize = 0, 
        Color Primary = null, 
        Color Secondary = null) 
        : base(parent, position, size, alignment)
    {
        this.fftLength = length;
        roundRadius = 5;

        // Init audio
        // Audio Capture

        if (Primary == null) this.Primary = Theme.Primary;
        else this.Primary = Primary;
        if (Secondary == null) this.Secondary = Theme.Secondary.Override(alpha: 0.5f);
        else this.Secondary = Secondary;

        if (averageAmpsSize != 0)
        {
            averageAmps = new float[averageAmpsSize];
            smoothAverageAmps = new float[averageAmpsSize];
            avAmpsMode = true;

        }

        fftValues = new float[length];
        barHeight = new float[length];

        var outputDevice = AudioDevices.GetOutputDeviceOrDefault();
        if (outputDevice == null) return;
        
        capture = new WasapiLoopbackCapture(outputDevice);
        capture.DataAvailable += OnDataAvailable;
        capture.StartRecording();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        try
        {
            if (capture != null)
            {
                capture.DataAvailable -= OnDataAvailable;
                capture.StopRecording();
                capture.Dispose();
            }
        }catch(ThreadInterruptedException e)
        {
            return;
        }
    }

    float updateTick = 0;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (capture == null) return;

        updateTick += 1000 * deltaTime;

        if (fftValues == null || fftValues.Length == 0) return;

        for (int i = 0; i < fftValues.Length; i++)
        {
            float amplitude = (float)Math.Sqrt(Math.Abs(fftValues[i]));
            amplitude *= 7.5f;
            amplitude = Math.Clamp(amplitude, 0, 1);

            if (float.IsNaN(amplitude) || float.IsInfinity(amplitude)) amplitude = 0f;

            barHeight[i] = MathRendering.LinearInterpolation(barHeight[i], amplitude, (amplitude > barHeight[i]) ? barUpSmoothing : barDownSmoothing * deltaTime);
        }

        if (avAmpsMode)
        {
            for (int a = 0; a < averageAmps.Length; a++)
            {
                smoothAverageAmps[a] = MathRendering.LinearInterpolation(smoothAverageAmps[a], averageAmps[a], barUpSmoothing * deltaTime);
            }

            if (avAmpsMode && (updateTick > 75))
            {
                updateTick = 0;
                for (int a = averageAmps.Length - 2; a >= 0; a--)
                {
                    averageAmps[a + 1] = averageAmps[a];
                }

                averageAmps[0] = AverageAmplitude;
            }
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var buffer = new float[e.Buffer.Length / 4];
        Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.Buffer.Length);

        // Apply a window function to the buffer, e.g., Hanning window
        var windowedBuffer = new float[fftLength];
        for (int i = 0; i < fftLength; i++)
        {
            if (i < buffer.Length)
                windowedBuffer[i] = buffer[i] * (0.5f * (1 - MathF.Cos(2 * MathF.PI * i / (fftLength - 1))));
        }

        // Perform FFT
        var complexBuffer = windowedBuffer.Select(value => new System.Numerics.Complex(value, 0)).ToArray();
        FFT(complexBuffer);

        // Get magnitude and apply frequency weighting
        lock (fftLock)
        {
            for (int i = 0; i < fftValues.Length; i++)
            {
                float magnitude = (float)complexBuffer[i].Magnitude;

                // Apply frequency weighting (logarithmic scaling)
                float scaleFactor = 1 + (float)Math.Log10(1 + i);
                fftValues[i] = magnitude * scaleFactor;
            }
        }
    }

    private void FFT(System.Numerics.Complex[] buffer)
    {
        int n = buffer.Length;
        int m = (int)Math.Log2(n);

        // Bit-reverse
        for (int i = 0; i < n; i++)
        {
            int j = BitReverse(i, m);
            if (j > i)
            {
                var temp = buffer[i];
                buffer[i] = buffer[j];
                buffer[j] = temp;
            }
        }

        // FFT
        for (int s = 1; s <= m; s++)
        {
            int mval = 1 << s;
            int mval2 = mval >> 1;
            var wm = new System.Numerics.Complex(Math.Cos(-2 * Math.PI / mval), Math.Sin(-2 * Math.PI / mval));
            for (int k = 0; k < n; k += mval)
            {
                var w = System.Numerics.Complex.One;
                for (int j = 0; j < mval2; j++)
                {
                    var t = w * buffer[k + j + mval2];
                    var u = buffer[k + j];
                    buffer[k + j] = u + t;
                    buffer[k + j + mval2] = u - t;
                    w *= wm;
                }
            }
        }
    }

    private int BitReverse(int n, int bits)
    {
        int reversedN = n;
        int count = bits - 1;

        n >>= 1;
        while (n > 0)
        {
            reversedN = (reversedN << 1) | (n & 1);
            count--;
            n >>= 1;
        }

        return ((reversedN << count) & ((1 << bits) - 1));
    }

    public int averageAmpModeRectSize = 24;

    float averageAmplitude = 0f;
    float[] averageAmps;
    float[] smoothAverageAmps;
    public float AverageAmplitude { get => averageAmplitude; }

    public Color Primary;
    public Color Secondary;

    public Color GetActionCol()
    {
        Color pColor = Color.LinearInterpolation(Secondary, Primary, averageAmplitude * 2);
        return pColor;
    }

    public Color GetInverseActionCol()
    {
        Color pColor = Color.LinearInterpolation(Primary, Secondary, averageAmplitude * 2);
        return pColor;
    }

    public override void Draw(SKCanvas canvas)
    {
        if (capture == null) return;

        lock (fftLock)
        {
            if (fftValues == null || fftValues.Length == 0) return;

            var width = Size.X;
            var height = Size.Y;

            var paint = GetPaint();

            for (int i = 0; i < barHeight.Length / divisor; i++)
            {
                averageAmplitude += MathRendering.Clamp(barHeight[i] * ((barHeight.Length - i) / barHeight.Length * 2 + 1f) * (Random.Shared.NextSingle() + 0.1f), 0, 1) * 4.5f;
            }

            if (!avAmpsMode)
            {
                var barWidth = width / (barHeight.Length / divisor);

                for (int i = 0; i < barHeight.Length / divisor; i++)
                {
                    float bH = (int)(barHeight[i] * height) + 1.5f; // Scale factor for visualization

                    var rect = SKRect.Create(Position.X + i * barWidth, Position.Y + (height / 2) - bH / 2, barWidth - 5f, bH);
                    var rRect = new SKRoundRect(rect, roundRadius);

                    Color pColor = Color.LinearInterpolation(Secondary, Primary, MathRendering.Clamp(barHeight[i], 0, 1));
                    paint.Color = GetColor(pColor).Value();

                    canvas.DrawRoundRect(rRect, paint);
                }
            }
            else
            {
                var barWidth = width / (smoothAverageAmps.Length / divisor);

                for (int i = 0; i < smoothAverageAmps.Length / divisor; i++)
                {
                    float bH = (int)(smoothAverageAmps[i] * height) + 1.5f; // Scale factor for visualization

                    var rect = SKRect.Create(Position.X + i * barWidth, Position.Y + (height / 2) - bH / 2, barWidth - 2.5f, bH);
                    var rRect = new SKRoundRect(rect, roundRadius);

                    Color pColor = Color.LinearInterpolation(Secondary, Primary, MathRendering.Clamp(smoothAverageAmps[i], 0, 1));
                    paint.Color = GetColor(pColor).Value();

                    canvas.DrawRoundRect(rRect, paint);
                }
            }

            averageAmplitude /= barHeight.Length;
        }
    }
}