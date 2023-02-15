using UnityEngine;

public abstract class TestBase : MonoBehaviour
{
	protected static void CreateGOChild(in Transform parent, out Transform InTransform, in string InGOName)
	{
		InTransform = new GameObject(InGOName).transform;
		InTransform.SetParent(parent);
		InTransform.localPosition = Vector3.zero;
	}

	protected static void Create3DCircleTransform(in Transform parent, out Transform InTransform, in string InGOName) => CreateGOChild(parent, out InTransform, InGOName);

	protected static void CreateSphereTransform(in Transform parent, out Transform outValue, in string InGOName)
	{
		outValue = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
		outValue.SetParent(parent);
		outValue.GetComponent<Collider>().Destroy();
		outValue.localPosition = Vector3.zero;
	}

	protected static void CreateCapsuleCollider(in Transform parent, out CapsuleCollider InOutData, in string InGOName)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		go.name = InGOName;
		go.transform.SetParent(parent);
		go.transform.localPosition = Vector3.zero;
		InOutData = go.GetComponent<CapsuleCollider>();
		InOutData.enabled = false;
		go.GetComponent<Renderer>().Destroy();
	}
}