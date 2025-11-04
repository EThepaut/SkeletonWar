using UnityEngine;

public class ChangeBulletColor : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    void Start()
    {
        ChangeColor();
    }
    void ChangeColor()
    {
        var mats = meshRenderer.materials;
        for(int i = 0; i < mats.Length; i++)
        {
            mats[i].color = Color.orangeRed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
