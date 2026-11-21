using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace Example
{
	public class MeshCutTest : MonoBehaviour
	{
		[SerializeField] private MeshFilter filter;
		[SerializeField] private MeshRenderer meshRenderer;

		private MeshCutter cutter;

		void Start()
		{
			cutter = new MeshCutter(filter.sharedMesh);
		}

		// Update is called once per frame
		void Update()
		{
			if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
			{
				Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
				if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
				{
					if (hit.collider.gameObject == gameObject)
					{
						Vector3 hitPoint = hit.point;

						List<Mesh> meshes = cutter.GetMeshes(hitPoint, transform, 0.3f, 3, 0.1f);

						meshRenderer.gameObject.SetActive(false);

						for (int i = 0; i < meshes.Count; i++)
						{
							CreateGameObject(meshes[i], i);
						}
					}
				}
			}
		}

		private void CreateGameObject(Mesh mesh, int i)
		{
			GameObject go = new GameObject("Mesh" + i);
			MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = this.meshRenderer.sharedMaterial;
			MeshFilter filter = go.AddComponent<MeshFilter>();
			filter.sharedMesh = mesh;
			go.transform.localScale = transform.localScale;
			go.transform.localPosition = transform.localPosition;
			go.AddComponent<Rigidbody>();
			MeshCollider collider = go.AddComponent<MeshCollider>();
			collider.convex = true;
		}
	}
}
