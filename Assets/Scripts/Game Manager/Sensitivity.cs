/*Created by Pawe³ Mularczyk*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerType
{
    Mouse,
    Gamepad,
}

public class Sensitivity : MonoBehaviour
{
    [SerializeField] private float _mouseSensX = 12f;
    [SerializeField] private float _mouseSensY = 12f;
    [SerializeField] private float _joystickSensX = 80f;
    [SerializeField] private float _joystickSensY = 55f;

    public Vector2 GetSensitivity(ControllerType controllerType)
    {
        Vector2 sensXY;

        switch (controllerType)
        {
            case ControllerType.Mouse:

                sensXY = new Vector2(_mouseSensX, _mouseSensY);
                return sensXY;

            case ControllerType.Gamepad:

                sensXY = new Vector2(_joystickSensX, _joystickSensY);
                return sensXY;
        }

        return Vector2.zero;
    }
}
