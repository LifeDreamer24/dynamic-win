using DynamicWin.UserSettings;

namespace DynamicWin.Rendering.Primitives;

public class SecondOrder(Vec2 x0, float f = 2f, float z = 0.4f, float r = 0.1f)
{
    private Vec2 _xp = x0;
    private Vec2 _y = x0;
    private Vec2 _yd = new(0, 0);
    
    private float _k1 = (float)(z / (Math.PI * f));
    private float _k2 = (float)(1 / (2 * Math.PI * f * (2 * Math.PI * f)));
    private float _k3 = (float)(r * z / (2 * Math.PI * f));

    public void SetValues(float f = 2f, float z = 0.4f, float r = 0.1f)
    {
        _k1 = (float)(z / (Math.PI * f));
        _k2 = (float)(1 / (2 * Math.PI * f * (2 * Math.PI * f)));
        _k3 = (float)(r * z / (2 * Math.PI * f));
    }

    public Vec2 Update(float T, Vec2 x, Vec2? xd = null)
    {
        if (!Settings.AllowAnimation) return x;

        if (xd != null)
        {
            xd = (x - _xp) / new Vec2(T, T);
            _xp = x;
        }
        
        var k2Stable = Math.Max(_k2, Math.Max(T * T / 2 + T * _k1 / 2, T * _k1));
        
        _y += new Vec2(T, T) * _yd;
        _yd += T * (x + new Vec2(_k3, _k3) * xd - _y - _k1 * _yd) / k2Stable;

        _y.X = MathRendering.LimitDecimalPoints(_y.X, 1);
        _y.Y = MathRendering.LimitDecimalPoints(_y.Y, 1);

        return _y;
    }
}