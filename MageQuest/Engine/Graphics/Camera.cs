// Modified from source: https://github.com/IrishBruse/LDtkMonogame/blob/main/LDtk.LevelViewer/Camera.cs
// License: MIT
// Licensed to: Ethan Conneely - IrishBruse

using System;
using Microsoft.Xna.Framework;

namespace MageQuest.Graphics;

public class Camera
{
    float currentShake;
    float shakeMagnitude;
    int shakeTime;
    FPoint _lastMousePosInWorld;

    static readonly FPoint halfScreen = new(Consts.ScreenWidthPixels / 2, Consts.ScreenHeightPixels / 2);

    public static FPoint HalfScreen => halfScreen;

    public const int BaseFocusSpeed = 16;

    public int FocusSpeed { get; set; } = BaseFocusSpeed;

    public FPoint TargetPosition { get; set; }

    public FPoint Position { get; set; }

    public float Zoom { get; set; } = 1;

    public Matrix Transform { get; private set; }

    public FPoint MousePositionInWorld => Input.InputDisabled ? _lastMousePosInWorld : Input.GetMousePositionWithZoom(Zoom, clamp: true) + Position;

    public FRectangle Deadzone { get; set; } = new(0, 32, 0, 4);

    public FRectangle Bounds { get; set; } = new(0, 0, Consts.ScreenWidthPixels, Consts.ScreenHeightPixels);

    public void SetShake(float shakeMagnitude, int shakeTime)
    {
        if(Math.Abs(shakeMagnitude) >= this.currentShake)
        {
            this.shakeMagnitude = Math.Abs(shakeMagnitude);
            this.currentShake = Math.Abs(shakeMagnitude);
            this.shakeTime = Math.Abs(shakeTime);
        }
    }

    public void AddShake(float shakeMagnitude, int shakeTime)
    {
        this.shakeMagnitude = Math.Abs(shakeMagnitude);
        this.currentShake += Math.Abs(shakeMagnitude);
        this.shakeTime = Math.Abs(shakeTime);
    }

    public void Update()
    {
        if(!Input.InputDisabled)
            _lastMousePosInWorld = MousePositionInWorld;

        var nextPos = Position;
        var targetPos = TargetPosition;

        // nextPos.Y = MathHelper.Clamp(targetPos.Y, -Deadzone.Y, Deadzone.Height);
        // nextPos.X = MathHelper.Clamp(targetPos.X, -Deadzone.X, Deadzone.Width);

        if(targetPos.Y < Position.Y - Deadzone.Y)
        {
            nextPos.Y = targetPos.Y + Deadzone.Y;
        }

        if(targetPos.Y > Position.Y + Deadzone.Height)
        {
            nextPos.Y = targetPos.Y - Deadzone.Height;
        }

        if(targetPos.X < Position.X - Deadzone.X)
        {
            nextPos.X = targetPos.X + Deadzone.X;
        }

        if(targetPos.X > Position.X + Deadzone.Width)
        {
            nextPos.X = targetPos.X - Deadzone.Width;
        }

        Position = FPoint.Clamp(nextPos, Bounds.Location + halfScreen, Bounds.Location + Bounds.Size - halfScreen);

        // screen space

        Vector2 basePosition = Vector2.Round((Position - halfScreen).ToVector2());

        var shakePosition = basePosition;
        if(currentShake != 0)
        {
            shakePosition -= new Vector2(
                (Random.Shared.NextSingle() - 0.5f) * 2 * currentShake,
                (Random.Shared.NextSingle() - 0.5f) * 2 * currentShake
            );
        }

        if(shakeTime > 0)
            currentShake = MathHelper.Max(0, currentShake - (1f / shakeTime * shakeMagnitude));
        else
            currentShake = 0;

        if(currentShake == 0)
            shakeTime = 0;

        var finalPosition = Vector2.Round(shakePosition);

        Transform = Matrix.CreateTranslation(new(-finalPosition, 0)) * Matrix.CreateScale(Zoom);
    }
}
