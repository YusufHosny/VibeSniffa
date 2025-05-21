using UnityEngine;
using TMPro;

public class InferenceBoxManager : MonoBehaviour
{
    public TextMeshPro TextLabel;
    public GameObject PanelMesh;


    public void Initialize(Vector3 worldPos, Quaternion rotation)
    {
        transform.position = worldPos;
        transform.rotation = rotation;
        PanelMesh.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, .8f);
        TextLabel.text = "Click To Sniff Vibes.";
    }

    private void LateUpdate()
    {
        var toCam = Camera.main.transform.position - transform.position;
        toCam.y = 0;
        transform.rotation = Quaternion.LookRotation(toCam);
    }

    public void ChangeColor(Color color)
    {
        PanelMesh.GetComponent<MeshRenderer>().material.color = color;
    }

    public void SetText(string text)
    {
        TextLabel.text = text;
    }
}
