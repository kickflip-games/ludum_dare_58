using UnityEngine;

namespace LudumDare58.Game
{
    public class DisableOnRetry : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.IsRetry)
            {
                gameObject.SetActive(false);
            }
        }
    }
}