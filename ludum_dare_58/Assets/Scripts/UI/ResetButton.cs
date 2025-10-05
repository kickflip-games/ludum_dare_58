using UnityEngine;
using TMPro;
using LudumDare58;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    public Vehicle vehicle;
    // button
    public Button button;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() => {
            vehicle.ResetUpright();
        });
    }

}
