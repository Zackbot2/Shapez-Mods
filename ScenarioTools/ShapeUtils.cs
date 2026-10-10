using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools
{
    public static class ShapeUtils
    {
        public static string RotateQuadShape(string shapeCode)
        {
            string newCode = "";
            string[] layers = shapeCode.Split(':');

            foreach (string layer in layers)
            {
                string newLayer = "";
                string lastTwoChars = layer[^2..];
                newLayer += lastTwoChars;
                newLayer += layer[..^2];
                newCode += newLayer + ":";
            }
            return newCode.TrimEnd(':');
        }

        public static string[] GetRotationsForQuadShape(string shapeCode)
        {
            string[] rotations = new string[4];
            rotations[0] = shapeCode;
            string? lastShape = null;
            for (int i = 1; i < 4; i++)
            {
                string? rotatedShape = RotateQuadShape(lastShape ?? shapeCode);

                if (rotations.Contains(rotatedShape)) break;

                rotations[i] = rotatedShape;
                lastShape = rotatedShape;
            }
            return rotations;
        }
    }
}
