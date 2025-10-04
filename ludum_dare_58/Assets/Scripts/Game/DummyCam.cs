using UnityEngine;

namespace LudumDare58
{
    public class DummyCam : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}