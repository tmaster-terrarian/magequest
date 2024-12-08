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

    static FPoint halfScreen = new(Consts.ScreenWidthPixels / 2, Consts.ScreenHeightPixels / 2);

    public const int BaseFocusSpeed = 16;

    public int FocusSpeed { get; set; } = BaseFocusSpeed;

    public FPoint TargetPosition { get; set; }

    public FPoint Position { get; set; }

    public FPoint WorldOrigin { get; set; }

    public float Zoom { get; set; } = 1;

    public Matrix Transform { get; private set; }

    public FPoint MousePositionInWorld => Input.InputDisabled ? _lastMousePosInWorld : Input.GetMousePositionWithZoom(Zoom, clamp: true) + Position;

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

        if(Position != TargetPosition - halfScreen)
        {
            if(Math.Abs(TargetPosition.X - Position.X - halfScreen.X) < FocusSpeed)
                nextPos.X = TargetPosition.X - halfScreen.X;
            else
                nextPos.X += (TargetPosition.X - Position.X - halfScreen.X) / FocusSpeed;

            if(Math.Abs(TargetPosition.Y - Position.Y - halfScreen.X) < FocusSpeed)
                nextPos.Y = TargetPosition.Y - halfScreen.Y;
            else
                nextPos.Y += (TargetPosition.Y - Position.Y - halfScreen.Y) / FocusSpeed;
        }

        Position = nextPos;

        var basePosition = Vector2.Round((Position - WorldOrigin).ToVector2());

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
