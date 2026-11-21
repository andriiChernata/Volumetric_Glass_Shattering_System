using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{
	public static Vector3 XY(this Vector3 origin)
	{
		return new Vector3(origin.x, origin.y, 0);
	}
}
