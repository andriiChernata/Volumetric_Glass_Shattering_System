using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MeshCutter
{
    private Mesh origin;
	private Vector3 rightVector;
	private Vector3 frontVector;
	private Vector3 upVector;

    public MeshCutter(Mesh mesh)
    {
        origin = mesh;
	}

    private Vector2 GetFrontUV(Vector3 point, bool flip = false)
    {
        Vector2 uv;

        uv.x = 1.0f-(point.x - origin.bounds.min.x) / (origin.bounds.max.x - origin.bounds.min.x);
        uv.y = 1.0f-(point.y - origin.bounds.min.y) / (origin.bounds.max.y - origin.bounds.min.y);

		if (flip)
			uv.y = 1.0f - uv.y;

        return uv;
	}

	private Vector2 GetSideUV(Vector3 point, bool flip = false)
	{
		Vector2 uv;

		uv.x = (point.z - origin.bounds.min.z) / (origin.bounds.max.z - origin.bounds.min.z);
		uv.y = (point.y - origin.bounds.min.y) / (origin.bounds.max.y - origin.bounds.min.y);

		if (flip)
			uv.x = 1.0f - uv.x;

		return uv;
	}

	private Vector2 GetUpUv(Vector3 point, bool flip = false)
	{
		Vector2 uv;

		uv.x = 1.0f - (point.x - origin.bounds.min.x) / (origin.bounds.max.x - origin.bounds.min.x);
		uv.y = 1.0f - (point.z - origin.bounds.min.z) / (origin.bounds.max.z - origin.bounds.min.z);

		if (flip)
			uv.y = 1.0f - uv.y;

		return uv;
	}

	private Vector3 GetUV(Vector3 normal, Vector3 point)
	{
		Vector3 target = Vector3.one;
		float maxDot = -1;

		CheckNormal(frontVector, normal, ref maxDot, ref target);
		CheckNormal(rightVector, normal, ref maxDot, ref target);
		CheckNormal(upVector, normal, ref maxDot, ref target);

		if (target == frontVector)
			return GetFrontUV(point);
		if (target == -frontVector)
			return GetFrontUV(point, true);


		if (target == rightVector)
			return GetSideUV(point);
		if (target == -rightVector)
			return GetSideUV(point, true);


		if (target == upVector)
			return GetUpUv(point);

		return GetUpUv(point, true);
	}

	private void CheckNormal(Vector3 dir, Vector3 normal, ref float maxDot, ref Vector3 target)
	{
		float dot = Vector3.Dot(normal, dir);

		if(maxDot < dot)
		{
			maxDot = dot;
			target = dir;
		}
		else
		{
			dot = Vector3.Dot(normal, -dir);

			if (maxDot < dot)
			{
				maxDot = dot;
				target = -dir;
			}
		}
	}

	private Vector3 GetNormal(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Vector3 v1 = point2 - point1;
        Vector3 v2 = point3 - point1;

        Vector3 cross = Vector3.Cross(v1, v2);

        return cross;
    }

    public List<Mesh> GetMeshes(Vector3 hitPoint,Transform originTransform, float lenght, int sectionAmount, float rndOffset)
    {
		rightVector = originTransform.right;
		frontVector = originTransform.forward;
		upVector = originTransform.up;

		hitPoint = originTransform.InverseTransformPoint(hitPoint);
		hitPoint.z = origin.bounds.min.z;

        int hAmount = (int)((origin.bounds.max.y - origin.bounds.min.y) / lenght);
        int wAmount = (int)((origin.bounds.max.x - origin.bounds.min.x) / lenght);

        hAmount = Mathf.Clamp(hAmount, 1, 1000);
		wAmount = Mathf.Clamp(wAmount, 1, 1000);

		Vector3[] totalPoints = new Vector3[(hAmount - 1) * 2 + (wAmount - 1) * 2 + 4];

        int pointIndex = 0;

        float hLenght = (origin.bounds.max.y - origin.bounds.min.y) / hAmount;
		float wLenght = (origin.bounds.max.x - origin.bounds.min.x) / wAmount;
		Vector3 hOffset = new Vector3(0, hLenght, 0);
		Vector3 wOffset = new Vector3(wLenght, 0, 0);

		//setup all point along the perimeter of the surface

		totalPoints[pointIndex++] = origin.bounds.min;
		for (int i = 0; i < hAmount - 1; i++)
		{
			totalPoints[pointIndex] = totalPoints[pointIndex - 1] + hOffset;
			totalPoints[pointIndex].y += Random.Range(-rndOffset, rndOffset);
			pointIndex++;
		}

		totalPoints[pointIndex] = origin.bounds.min;
		totalPoints[pointIndex].y = origin.bounds.max.y;
		pointIndex++;
		for (int i = 0; i < wAmount - 1; i++)
		{
			totalPoints[pointIndex] = totalPoints[pointIndex - 1] + wOffset;
			totalPoints[pointIndex].x += Random.Range(-rndOffset, rndOffset);
			pointIndex++;
		}

		totalPoints[pointIndex] = origin.bounds.max;
		totalPoints[pointIndex].z = origin.bounds.min.z;
		pointIndex++;
		for (int i = 0; i < hAmount - 1; i++)
		{
			totalPoints[pointIndex] = totalPoints[pointIndex - 1] - hOffset;
			totalPoints[pointIndex].y += Random.Range(-rndOffset, rndOffset);
			pointIndex++;
		}

		totalPoints[pointIndex] = origin.bounds.max;
		totalPoints[pointIndex].y = origin.bounds.min.y;
		totalPoints[pointIndex].z = origin.bounds.min.z;
		pointIndex++;
		for (int i = 0; i < wAmount - 1; i++)
		{
			totalPoints[pointIndex] = totalPoints[pointIndex - 1] - wOffset;
			totalPoints[pointIndex].x += Random.Range(-rndOffset, rndOffset);
			pointIndex++;
		}

		List<Mesh> meshes = new List<Mesh>((hAmount * 2 + wAmount * 2) * sectionAmount);

        pointIndex = 0;

        float depth = origin.bounds.max.z - origin.bounds.min.z;

		for (int i=0; i< (hAmount * 2 + wAmount * 2) - 1; i++)
        {
			meshes.AddRange(GetSectionMeshes(new Vector3[] { hitPoint, totalPoints[pointIndex], totalPoints[pointIndex+1] }, depth, sectionAmount));
            pointIndex++;
		}

		meshes.AddRange(GetSectionMeshes(new Vector3[] { hitPoint, totalPoints[pointIndex], totalPoints[0] }, depth, sectionAmount));

        return meshes;
    }

	private Vector3[] GetSectionPoints(Vector3[] points, int sectionAmount, int sectionIndex)
	{
		if (sectionIndex == 0)
			return new Vector3[] {points[0],
					points[0] + (points[1] - points[0]).XY() / sectionAmount,
					points[0] + (points[2] - points[0]).XY() / sectionAmount};

		if (sectionIndex == sectionAmount - 1)
			return new Vector3[] {points[1] - (points[1] - points[0]).XY() / sectionAmount,
				points[1],
				points[2],
				points[2] - (points[2] - points[0]).XY() / sectionAmount};

		return new Vector3[] {points[0] + sectionIndex * (points[1] - points[0]).XY() / sectionAmount,
			points[0] + (sectionIndex + 1) * (points[1] - points[0]).XY() / sectionAmount,
			points[0] + (sectionIndex + 1) * (points[2] - points[0]).XY() / sectionAmount,
		points[0] + sectionIndex * (points[2] - points[0]).XY() / sectionAmount
		};
	}

	private Mesh[] GetSectionMeshes(Vector3[] points, float depth, int sectionAmount)
	{
		Mesh[] meshes = new Mesh[sectionAmount];

		meshes[0] = CreateTriangleMesh(GetSectionPoints(points, sectionAmount, 0), depth);

		for(int i=1; i<sectionAmount; i++)
		{
			meshes[i] = CreateTrapezeMesh(GetSectionPoints(points, sectionAmount, i), depth);
		}

		return meshes;
	}

	private Mesh CreateTrapezeMesh(Vector3[] points, float depth)
	{
		Mesh mesh = new Mesh();

		Vector3 offset = new Vector3(0, 0, -depth);

		Vector3[] verticles = new Vector3[24];
		Vector2[] uv = new Vector2[24];
		Vector3[] normals = new Vector3[24];
		int[] triangles = new int[36];

		int verticleIndex = 0;
		int trianglIndex = 0;

		//set front face

		AddTrapezFront(points[0], points[1], points[2], points[3],
			ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex,ref trianglIndex);

		//set back face

		AddTrapezFront(points[0] - offset, points[1] - offset, points[2] - offset, points[3] - offset,
			ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex, true);

		//set sides

		AddSide(points[0], points[1], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);
		AddSide(points[1], points[2], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);
		AddSide(points[2], points[3], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);
		AddSide(points[3], points[0], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);

		mesh.vertices = verticles;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();

		return mesh;
	}

	private Mesh CreateTriangleMesh(Vector3[] points, float depth)
    {
        Mesh mesh = new Mesh();

        Vector3 offset = new Vector3(0, 0, -depth);

        Vector3[] verticles = new Vector3[18];
		Vector2[] uv = new Vector2[18];
        Vector3[] normals = new Vector3[18];
        int[] triangles = new int[24];

        int verticleIndex = 0;
        int trianglIndex = 0;

        Vector3 normal = GetNormal(points[0], points[1], points[2]).normalized;

		//set front face

		for (int i=0; i<3; i++)
        {
            uv[verticleIndex] = GetFrontUV(points[i]);
            normals[verticleIndex] = normal;
            triangles[trianglIndex++] = verticleIndex;
			verticles[verticleIndex++] = points[i];
        }

		//set back face

		for (int i = 2; i >= 0; i--)
		{
			uv[verticleIndex] = GetFrontUV(points[i], true);
			normals[verticleIndex] = -normal;
			triangles[trianglIndex++] = verticleIndex;
			verticles[verticleIndex++] = points[i] - offset;
		}

        //set sides

        AddSide(points[0], points[1], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);
		AddSide(points[1], points[2], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);
		AddSide(points[2], points[0], offset, ref uv, ref normals, ref triangles, ref verticles, ref verticleIndex, ref trianglIndex);

		mesh.vertices = verticles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

		return mesh;
	}

    private void AddSide(in Vector3 point1, in Vector3 point4, in Vector3 offset, ref Vector2[] uv, 
        ref Vector3[] normals, ref int[] triangles, ref Vector3[] verticles, ref int verticleIndex, ref int trianglIndex)
    {
        Vector3 point2 = point1 - offset;
        Vector3 point3 = point4 - offset;

        Vector3 normal = GetNormal(point1, point2, point4).normalized;

		int startIndex = verticleIndex;

        triangles[trianglIndex++] = verticleIndex;
		triangles[trianglIndex++] = verticleIndex + 1;
		triangles[trianglIndex++] = verticleIndex + 3;

		triangles[trianglIndex++] = verticleIndex + 1;
		triangles[trianglIndex++] = verticleIndex + 2;
		triangles[trianglIndex++] = verticleIndex + 3;

		uv[verticleIndex] = GetUV(normal, point1);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point1;

		uv[verticleIndex] = GetUV(normal, point2);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point2;

		uv[verticleIndex] = GetUV(normal, point3);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point3;

		uv[verticleIndex] = GetUV(normal, point4);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point4;
	}

	private void AddTrapezFront(in Vector3 point1, in Vector3 point2, in Vector3 point3, in Vector3 point4, ref Vector2[] uv,
		ref Vector3[] normals, ref int[] triangles, ref Vector3[] verticles, ref int verticleIndex, ref int trianglIndex, bool isBack = false)
	{
		Vector3 normal = GetNormal(point1, point2, point4).normalized;

		int startIndex = verticleIndex;

		if (isBack)
		{
			normal = -normal;

			triangles[trianglIndex++] = verticleIndex + 3;
			triangles[trianglIndex++] = verticleIndex + 1;
			triangles[trianglIndex++] = verticleIndex;


			triangles[trianglIndex++] = verticleIndex + 3;
			triangles[trianglIndex++] = verticleIndex + 2;
			triangles[trianglIndex++] = verticleIndex + 1;
		}
		else
		{
			triangles[trianglIndex++] = verticleIndex;
			triangles[trianglIndex++] = verticleIndex + 1;
			triangles[trianglIndex++] = verticleIndex + 3;


			triangles[trianglIndex++] = verticleIndex + 1;
			triangles[trianglIndex++] = verticleIndex + 2;
			triangles[trianglIndex++] = verticleIndex + 3;
		}

		uv[verticleIndex] = GetFrontUV(point1, isBack);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point1;

		uv[verticleIndex] = GetFrontUV(point2, isBack);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point2;

		uv[verticleIndex] = GetFrontUV(point3, isBack);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point3;

		uv[verticleIndex] = GetFrontUV(point4, isBack);
		normals[verticleIndex] = normal;
		verticles[verticleIndex++] = point4;
	}

}
