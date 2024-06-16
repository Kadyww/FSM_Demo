
using UnityEngine;

public static class Formation
{
    public static Vector3[] GenerateSquareFormation(Vector3 firstLineCenter, Vector3 direction, int num_of_points, float radius, float ratio = 16 / 9f)
    {
        // Calculate the number of columns and rows
        int numColumns = Mathf.CeilToInt(Mathf.Sqrt(num_of_points * ratio));
        int numRows = Mathf.CeilToInt(num_of_points / (float)numColumns);

        // Calculate the right direction based on the forward direction
        Vector3 rightDirection = Vector3.Cross(direction, Vector3.up).normalized;

        // Adjust the directions to ensure they are perpendicular and normalized
        direction = direction.normalized;
        rightDirection = rightDirection.normalized;

        Vector3[] positions = new Vector3[num_of_points];

        // Calculate the starting point of the formation
        Vector3 startPoint = firstLineCenter - (rightDirection * ((numColumns - 1) * radius * 0.5f)) - (direction * ((numRows - 1) * radius * 0.5f));

        int pointIndex = 0;

        // Generate points in a grid
        for (int row = 0; row < numRows; row++)
        {
            int pointsInRow = Mathf.Min(numColumns, num_of_points - pointIndex);
            float rowOffset = (numColumns - pointsInRow) * radius * 0.5f; // Center the row if it is the last row and not fully filled

            for (int col = 0; col < pointsInRow; col++)
            {
                Vector3 position = startPoint + (rightDirection * (col * radius + rowOffset)) + (direction * (row * radius));
                positions[pointIndex] = position;
                pointIndex++;
            }
        }

        return positions;
    }

    public static Vector3[] AssignUnitsToFormationPoints(Vector3[] unitPositions, Vector3[] formationPoints)
    {
        if (unitPositions.Length != formationPoints.Length)
        {
            Debug.LogError("Unit positions and formation points must have the same length");
            return null;
        }
        
        // Create a list to keep track of assigned points
        bool[] assignedPoints = new bool[formationPoints.Length];
        
        for (int i = 0; i < unitPositions.Length; i++)
        {
            float closestDistance = float.MaxValue;
            int closestIndex = -1;

            for (int j = 0; j < formationPoints.Length; j++)
            {
                if (assignedPoints[j]) continue;

                float distance = Vector3.Distance(unitPositions[i], formationPoints[j]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = j;
                }
            }

            if (closestIndex != -1)
            {
                assignedPoints[closestIndex] = true;
                unitPositions[i] = formationPoints[closestIndex];
            }
        }
        
        return unitPositions;
    }
}
