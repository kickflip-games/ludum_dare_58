using LudumDare58.Game;
using UnityEngine;

namespace LudumDare58.UI
{
    public class LevelNumberDisplay : MonoBehaviour
    {
        [SerializeField]
        LevelData levelData = null;

        [SerializeField]
        TMPro.TextMeshProUGUI text = null;


        void Start()
        {
            int levelNumber = levelData.GetLevelNumber(gameObject.scene);

            text.text = $"Level {levelNumber}";
        }
    }
}