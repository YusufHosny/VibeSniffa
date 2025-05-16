using UnityEngine;
using TMPro;

public class InferenceBoxManager : MonoBehaviour
{
    public TextMeshPro TextLabel;

    public System.Action<InferenceBoxManager> OnClicked;

    public void Initialize(string label, Vector3 worldPos, Quaternion rotation)
    {
        TextLabel.text = label;
        transform.position = worldPos;
        transform.rotation = rotation;
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(this);
    }
}
